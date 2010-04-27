(define-module (mono mono)
  :use-module (oop goops)
  :use-module (oop goops internal)
  :use-module (guile-mono))

(%init-mono-module)
;(%boot-mono "Test.exe")

(re-export <gw:mono:domain> <gw:mono:assembly> <gw:mono:image> <gw:mono:class>
	   <MonoObject> <gw:mono:method>
	   mono-jit-init mono-domain-get mono-domain-assembly-open
	   mono-assembly-get-image mono-assembly-find-class mono-class-from-name
	   %mono-get-boxed-int32 %mono-type-get-kind
	   %mono-delegate-create %mono-delegate-invoke
	   %boot-mono
	   )

(export mono-class-get-methods <mono:class> <mono:method>
	mono-method-get-name mono-method-is-static? mono-method-get-class
	mono-method-can-invoke? mono-method-get-signature print
	<mono:method-signature> mono-method-signature-get-return-type
	mono-method-signature-has-this? mono-method-signature-get-param-types
	mono-class-get-method-hash mono-error mono-class-get-type
	<mono:type> mono-type-get-name invoke
	mono-class-get-name <mono:metaclass>
	make-mono-method <mono:object> make-mono-class
	mono-runtime-invoke mono-runtime-invoke-static
	mono-class-get-properties <mono:property> mono-property-get-name
	mono-property-get-getter mono-property-get-setter mono-property-get-class
	<mono:valuetype> mono-valuetype-get-type mono-valuetype-get-name
	mono-delegate-create
	)

(define-public (mono-assembly-find-type assembly nspace name)
  (let* ((klass (mono-assembly-find-class assembly nspace name))
	 (type (%mono-class-get-type klass)))
    (make <mono:type> #:type type)))

(define (mono-error format-string . args)
  (save-stack)
  (scm-error 'mono-error #f format-string args '()))

(define (%mono-class-get-methods class)
  (let* ((klass (class-slot-ref class 'klass))
	 (num-methods (%mono-class-num-methods klass))
	 (methods (make-vector num-methods)))
    (do ((idx 0 (+ idx 1))) ((>= idx num-methods) methods)
      (let* ((method (%mono-class-get-method klass idx))
	     (instance (make <mono:method> #:method method #:klass class)))
	(vector-set! methods idx instance)))
    methods))

(define (%mono-class-get-properties class)
  (let* ((klass (class-slot-ref class 'klass))
	 (num-properties (%mono-class-num-properties klass))
	 (properties (make-vector num-properties)))
    (do ((idx 0 (+ idx 1))) ((>= idx num-properties) properties)
      (let* ((property (%mono-class-get-property klass idx))
	     (instance (make <mono:property> #:property property #:klass class)))
	(vector-set! properties idx instance)))
    properties))

(define (%mono-class-get-property-names klass)
  (let* ((num-properties (%mono-class-num-properties klass))
	 (names (make-vector num-properties)))
    (do ((idx 0 (+ idx 1))) ((>= idx num-properties) names)
      (let* ((property (%mono-class-get-property klass idx))
	     (name (%mono-property-get-name property)))
	(vector-set! names idx name)))
    names))

(define-class <mono:metaclass> (<class>)
  (%num-fields #:allocation #:class #:init-value 0))

(define-class <mono:class> (<mono:metaclass>))

(define-class <mono:baseclass> ()
  (%fields)
  #:metaclass <mono:metaclass>)

(define-method (compute-get-n-set (class <mono:metaclass>) s)
  (let ((slot-name (slot-definition-name s))
	(init (get-keyword #:my-init (slot-definition-options s) *unspecified*)))
    (if (unspecified? init)
	(next-method)
	(case (slot-definition-allocation s)
	  ((#:instance)
	   (let* ((num-fields (slot-ref class '%num-fields))
		  (new-num-fields (+ num-fields 1)))
	     (slot-set! class '%num-fields new-num-fields)
	     (list (lambda (o)
		     (let* ((vector (slot-ref o '%fields)))
		       (and (unspecified? (vector-ref vector num-fields))
			    (vector-set! vector num-fields (init o)))
		       (vector-ref vector num-fields)))
		   (lambda (o v) (mono-error "Read-only slot: ~S" slot-name)))))

	  ((#:each-subclass)
	   (let ((closure (make-closure-variable class)))
	     (list (lambda (o)
		     (and (unbound? ((car closure) class))
			  ((cadr closure) class (init class)))
		     ((car closure) class))
		   (lambda (o v) (mono-error "Read-only slot: ~S" slot-name)))))

	  (else
	   (mono-error "Unknown allocation: ~S" allocation))))))

(define-method (compute-get-n-set (class <mono:class>) s)
  (let ((slot-name (slot-definition-name s)))
    (case (slot-definition-allocation s)
      ((#:mono-property)
       (let loop ((l (class-precedence-list class)))
	 (let ((this (car l)))
	   (if (memq slot-name (map slot-definition-name (class-direct-slots this)))
	       (list (lambda (o)
		       (let* ((hash (class-slot-ref this 'method-hash))
			      (accessor (string->symbol (string-append "get_" (symbol->string slot-name))))
			      (handle (hashq-get-handle hash accessor)))
			 (or handle (mono-error "Property has no `get' accessor: ~S" slot-name))
			 (let* ((method (cdr handle)) (is-static (mono-method-is-static? method)))
			   (if is-static
			       (invoke method '())
			       (invoke method (list o))))))
		     (lambda (o v)
		       (let* ((hash (class-slot-ref this 'method-hash))
			      (accessor (string->symbol (string-append "set_" (symbol->string slot-name))))
			      (handle (hashq-get-handle hash accessor)))
			 (or handle (mono-error "Property has no `set' accessor: ~S" slot-name))
			 (let* ((method (cdr handle)) (is-static (mono-method-is-static? method)))
			   (if is-static
			       (invoke method (list v))
			       (invoke method (list o v)))))))
	       (loop (cdr l))))))

      (else
       (next-method)))))

(define (property-slot-g-n-s class slot-name)
  (let* ((this-slot (assq slot-name (slot-ref class 'slots)))
	 (g-n-s (cddr (or (assq slot-name (slot-ref class 'getters-n-setters))
			  (slot-missing class slot-name)))))
    (if (not (memq (slot-definition-allocation this-slot)
		   '(#:mono-property)))
	(slot-missing class slot-name))
    g-n-s))

(define-public (mono-class-property-ref class slot)
  (let ((x ((car (property-slot-g-n-s class slot)) *unspecified*)))
    (if (unbound? x)
	(slot-unbound class slot)
	x)))

(define-public (mono-class-property-set! class slot value)
  ((cadr (property-slot-g-n-s class slot)) *unspecified* value))

(define-class <mono:type> (<mono:baseclass>)
  (type #:init-keyword #:type)
  (type-name #:allocation #:instance
	#:my-init (lambda (o) (%mono-type-get-name (slot-ref o 'type)))
	#:getter mono-type-get-name)
  #:metaclass <mono:metaclass>
  )

(define-method (write (obj <mono:type>) file)
  (display "#<" file)
  (display (class-name (class-of obj)) file)
  (display #\space file)
  (display (mono-type-get-name obj) file)
  (display #\> file))

(define-class <mono:method> (<mono:baseclass>)
  (method #:init-keyword #:method)
  (klass #:init-keyword #:klass #:getter mono-method-get-class)
  (is-accessor #:init-keyword #:is-accessor #:init-value #f)
  (method-name #:allocation #:instance #:my-init
	       (lambda (o) (%mono-method-get-name (slot-ref o 'method)))
	       #:getter mono-method-get-name)
  (is-static #:allocation #:instance #:my-init
	     (lambda (o) (%mono-method-is-static? (slot-ref o 'method)))
	     #:getter mono-method-is-static?)
  (can-invoke #:allocation #:instance #:my-init
	     (lambda (o) (%mono-method-can-invoke? (slot-ref o 'method)
						   (slot-ref o 'is-accessor)))
	     #:getter mono-method-can-invoke?)
  (signature #:allocation #:instance #:my-init
	     (lambda (o) (let ((sig (%mono-method-get-signature (slot-ref o 'method))))
			   (make <mono:method-signature> #:signature sig)))
	     #:getter mono-method-get-signature)
  #:metaclass <mono:metaclass>
  )

(define-class <mono:method-signature> (<mono:baseclass>)
  (signature #:init-keyword #:signature)
  (return-type #:allocation #:instance #:my-init
	       (lambda (o)
		 (let ((type (%mono-method-signature-get-return-type (slot-ref o 'signature))))
		   (make <mono:type> #:type type)))
	       #:getter mono-method-signature-get-return-type)
  (has-this #:allocation #:instance #:my-init
	    (lambda (o) (%mono-method-signature-has-this? (slot-ref o 'signature)))
	    #:getter mono-method-signature-has-this?)
  (param-types #:allocation #:instance #:my-init
	       (lambda (o)
		 (let* ((sig (slot-ref o 'signature))
			(count (%mono-method-signature-param-count sig))
			(vector (make-vector count)))
		   (do ((idx 0 (+ idx 1))) ((>= idx count) vector)
		     (let* ((ptype (%mono-method-signature-get-param sig idx))
			    (instance (make <mono:type> #:type ptype)))
		       (vector-set! vector idx instance)))))
	       #:getter mono-method-signature-get-param-types)
  #:metaclass <mono:metaclass>
  )

(define-class <mono:property> (<mono:baseclass>)
  (klass #:init-keyword #:klass #:getter mono-property-get-class)
  (property #:init-keyword #:property)
  (property-name #:allocation #:instance #:my-init
	       (lambda (o) (%mono-property-get-name (slot-ref o 'property)))
	       #:getter mono-property-get-name)
  (getter #:allocation #:instance #:my-init
	  (lambda (o) (let* ((getter (%mono-property-get-getter (slot-ref o 'property))))
			(and getter
			     (make <mono:method> #:method getter #:klass (slot-ref o 'klass)
				   #:is-accessor #t))))
	  #:getter mono-property-get-getter)
  (setter #:allocation #:instance #:my-init
	  (lambda (o) (let* ((setter (%mono-property-get-setter (slot-ref o 'property))))
			(and setter
			     (make <mono:method> #:method setter #:klass (slot-ref o 'klass)
				   #:is-accessor #t))))
	  #:getter mono-property-get-setter)
  #:metaclass <mono:metaclass>
  )

(define-method (write (obj <mono:method>) file)
  (display "#<" file)
  (display (class-name (class-of obj)) file)
  (display #\space file)
  (display (mono-method-get-class obj) file)
  (display #\space file)
  (display (mono-method-get-name obj) file)
  (display #\space file)
  (display (mono-method-is-static? obj) file)
  (display #\space file)
  (display (mono-method-can-invoke? obj) file)
  (display #\> file))

(define-method (write (obj <mono:method-signature>) file)
  (display "#<" file)
  (display (class-name (class-of obj)) file)
  (display #\space file)
  (display (mono-method-signature-get-return-type obj) file)
  (display #\space file)
  (display (mono-method-signature-has-this? obj) file)
  (display #\space file)
  (display (mono-method-signature-get-param-types obj) file)
  (display #\> file))

(define-method (write (obj <mono:property>) file)
  (display "#<" file)
  (display (class-name (class-of obj)) file)
  (display #\space file)
  (display (mono-property-get-name obj) file)
  (display #\> file))

(define mono-object-class
  (let ((klass *unspecified*))
    (lambda ()
      (and (unspecified? klass)
	   (set! klass (make <mono:class> #:klass (%mono-defaults-object-class))))
      klass)))

(define mono-object-type
  (let ((type *unspecified*))
    (lambda ()
      (and (unspecified? type)
	   (let* ((klass (%mono-defaults-object-class))
		  (otype (%mono-class-get-type klass)))
	     (set! type (make <mono:type> #:type otype))))
      type)))

(define mono-object-to-string-method
  (let ((to-string *unspecified*))
    (lambda ()
      (and (unspecified? to-string)
	   (let* ((methods (mono-class-get-methods (mono-object-class)))
		  (count (vector-length methods)))
	     (do ((idx 0 (+ idx 1))) ((>= idx count) to-string)
	       (let* ((method (vector-ref methods idx))
		      (name (mono-method-get-name method)))
		 (and (equal? name "ToString")
		      (set! to-string method))))
	     (and (unspecified? to-string)
		  (mono-error "Can't find Object.ToString"))))
      to-string)))

(define-generic print)

(define-class <mono:object> (<mono:baseclass>)
  (klass #:allocation #:each-subclass #:init-keyword #:klass)
  (object #:init-keyword #:object)
  (type #:allocation #:each-subclass #:my-init
	(lambda (class) (let ((type (%mono-class-get-type (class-slot-ref class 'klass))))
		      (make <mono:type> #:type type)))
	#:getter mono-class-get-type)
  (klass-name #:allocation #:each-subclass #:my-init
	(lambda (class) (%mono-class-get-name (class-slot-ref class 'klass)))
	#:getter mono-class-get-name)
  (methods #:allocation #:each-subclass #:my-init
	   (lambda (class) (%mono-class-get-methods class))
	   #:getter mono-class-get-methods)
  (method-hash #:allocation #:each-subclass #:my-init
	       (lambda (class)
		 (let* ((methods (class-slot-ref class 'methods))
			(count (vector-length methods))
			(hash (make-hash-table count)))
		   (begin
		     (do ((idx 0 (+ idx 1))) ((>= idx count) hash)
		       (let* ((method (vector-ref methods idx))
			      (name (mono-method-get-name method))
			      (symbol (string->symbol name)))
			 (hashq-create-handle! hash symbol method))))))
	       #:getter mono-class-get-method-hash)
  (properties #:allocation #:each-subclass #:my-init
	   (lambda (class) (%mono-class-get-properties class))
	   #:getter mono-class-get-properties)
  #:metaclass <mono:class>)
(class-slot-set! <mono:object> 'klass (%mono-defaults-object-class))

(define-generic get-mono-class)
(define-generic make-mono-class)
(define-generic make-mono-method)
(define-generic make-mono-property)

(define (get-parent-type type)
  (let* ((gw-type (slot-ref type 'type))
	 (gw-class (%mono-class-type-get-class gw-type))
	 (gw-parent-class (%mono-class-get-parent gw-class))
	 (gw-parent (%mono-class-get-type gw-parent-class))
	 (parent (make <mono:type> #:type gw-parent)))
    parent))

(define (get-parent-class type module)
  (let* ((parent-type (get-parent-type type))
	 (kind (%mono-type-get-kind (slot-ref parent-type 'type))))
    (cond
     ((eq? kind 28)
      <mono:object>)

     ((eq? kind 18)
      (make-mono-class parent-type module))

     (else
      (mono-error "not a class type (kind is ~S): ~S" kind type)))))

(define (map-to-scheme-class klass)
  (cond
   ((equal? klass <mono:boolean>)
    <boolean>)
   ((equal? klass <mono:char>)
    <char>)
   ((equal? klass <mono:sbyte>)
    <integer>)
   ((equal? klass <mono:byte>)
    <integer>)
   ((equal? klass <mono:short>)
    <integer>)
   ((equal? klass <mono:ushort>)
    <integer>)
   ((equal? klass <mono:int>)
    <integer>)
   ((equal? klass <mono:uint>)
    <integer>)
   ((equal? klass <mono:long>)
    <integer>)
   ((equal? klass <mono:ulong>)
    <integer>)
   ((equal? klass <mono:single>)
    <real>)
   ((equal? klass <mono:double>)
    <real>)
   (else
    klass)))

(define-method (make-mono-method (method <mono:method>) (module <module>))
  (let* ((klass (mono-method-get-class method))
	 (klass-name (class-slot-ref klass 'klass-name))
	 (method-name (string-append klass-name ":" (mono-method-get-name method)))
	 (symbol (string->symbol method-name))
	 (signature (mono-method-get-signature method))
	 (is-static (mono-method-is-static? method)))
    (or (module-defined? module symbol)
	(begin
	  (module-define! module symbol (make <generic> #:name symbol))
	  (module-export! module (list symbol))))
    (let* ((param-classes (map (lambda (param-type)
				 (get-mono-class param-type module))
			       (vector->list (mono-method-signature-get-param-types signature))))
	   (param-names (let ((i 0))
			  (map (lambda (param-type)
				 (set! i (+ i 1))
				 (string-append "param" (number->string i)))
			       param-classes)))
	   (all-param-classes (if is-static
				  param-classes
				  (append (list klass) param-classes)))
	   (all-param-names (if is-static
				param-names
				(append '(obj) param-names)))
	   (modified #f)
	   (simple-param-classes (map (lambda (klass)
					(let* ((scheme-klass (map-to-scheme-class klass)))
					  (or (equal? klass scheme-klass)
					      (set! modified #t))
					  scheme-klass))
				      all-param-classes))
	   (generic (module-symbol-binding module symbol))
	   (themethod (make-method all-param-classes
				   (lambda args
				     (invoke method args)))))
      (add-method! generic themethod)
      (and modified
	   (add-method! generic (make-method simple-param-classes
					     (lambda args
					       (invoke method args)))))
      themethod)))

(define-method (make-mono-property (property <mono:property>) (module <module>))
  (let* ((klass (mono-property-get-class property))
	 (name (mono-property-get-name property))
	 (getter (mono-property-get-getter property))
	 (setter (mono-property-get-setter property))
	 (can-invoke (and (if getter (mono-method-can-invoke? getter) #t)
			  (if setter (mono-method-can-invoke? setter) #t))))
    (and can-invoke
	 (let* ((type (if getter
			  (mono-method-signature-get-return-type
			   (mono-method-get-signature getter))
			  (vector-ref (mono-method-signature-get-param-types
				       (mono-method-get-signature setter)) 0)))
		(kind (%mono-type-get-kind (slot-ref type 'type))))
	   (get-mono-class type module)))))

(define-method (get-mono-class (type <mono:type>))
  (get-mono-class type (current-module)))

(define-method (get-mono-class (type <mono:type>) (module <module>))
  (let* ((gw-type (slot-ref type 'type))
	 (kind (%mono-type-get-kind gw-type))
	 (type-name (mono-type-get-name type))
	 (klass-name (string->symbol (string-append "<" type-name ">"))))
    (case kind
      ((18)
       (make-mono-class type module))

      ((2)
       <mono:boolean>)
      ((3)
       <mono:char>)
      ((4)
       <mono:sbyte>)
      ((5)
       <mono:byte>)
      ((6)
       <mono:short>)
      ((7)
       <mono:ushort>)
      ((8)
       <mono:int>)
      ((9)
       <mono:uint>)
      ((10)
       <mono:long>)
      ((11)
       <mono:ulong>)
      ((12)
       <mono:single>)
      ((13)
       <mono:double>)
      ((14)
       <mono:string>)

      ((28)
       <mono:object>)

      (else
       (mono-error "unknown class type (kind is ~S): ~S" kind type)))))

(define-method (make-mono-class (type <mono:type>))
  (make-mono-class type (current-module)))

(define-method (make-mono-class (type <mono:type>) (module <module>))
  (let* ((gw-type (slot-ref type 'type))
	 (kind (%mono-type-get-kind gw-type))
	 (type-name (mono-type-get-name type))
	 (klass-name (string->symbol (string-append "<" type-name ">"))))
    (or (eq? kind 18)
	(mono-error "not a class type (kind is ~S): ~S" kind type))
    (or (module-defined? module klass-name)
	(let* ((parent (get-parent-class type module))
	       (gw-class (%mono-class-type-get-class gw-type))
	       (gw-parent (%mono-class-get-parent gw-class))
	       (property-names (%mono-class-get-property-names gw-class))
	       (slots (map (lambda (name)
			     (list (string->symbol name) #:allocation #:mono-property))
			   (vector->list property-names)))
	       (klass (make-class (list parent) slots #:name klass-name)))
	  (module-define! module klass-name klass)
	  (module-export! module (list klass-name))
	  (class-slot-set! klass 'klass gw-class)
	  (map (lambda (method)
		 (and (%mono-method-can-invoke? (slot-ref method 'method) #f)
		      (make-mono-method method module)))
	       (vector->list (class-slot-ref klass 'methods)))
	  (map (lambda (property)
		 (and (%mono-property-can-invoke? (slot-ref property 'property))
		      (make-mono-property property module)))
	       (vector->list (class-slot-ref klass 'properties)))
	  (and (equal? gw-parent (%mono-defaults-multicast-delegate-class))
	       (let* ((invoke (%mono-get-delegate-invoke gw-class))
		      (invoke-method (make <mono:method> #:method invoke #:klass klass)))
		 (make-mono-method invoke-method module)))
	  )
	)
    (module-symbol-binding module klass-name)))

(define-method (initialize (obj <mono:object>) . initargs)
  (let ((obj (next-method)))
    (or (slot-bound? obj 'object)
	(mono-error "Missing #:object keyword"))
    obj))

(define-method (initialize (obj <mono:baseclass>) . initargs)
  (let* ((obj (next-method))
	 (class (class-of obj))
	 (num-fields (slot-ref class '%num-fields)))
    (slot-set! obj '%fields (make-vector num-fields *unspecified*))
    obj))

(define-class <mono:valuetype> (<mono:baseclass>)
  (klass #:allocation #:each-subclass #:init-keyword #:klass)
  (object #:init-keyword #:object)
  (type #:allocation #:each-subclass #:my-init
	(lambda (class) (let ((type (%mono-class-get-type (class-slot-ref class 'klass))))
			  (make <mono:type> #:type type)))
	#:getter mono-valuetype-get-type)
  (klass-name #:allocation #:each-subclass #:my-init
	      (lambda (class) (%mono-class-get-name (class-slot-ref class 'klass)))
	      #:getter mono-valuetype-get-name)
  )
(class-slot-set! <mono:valuetype> 'klass (%mono-defaults-object-class))

(export <mono:fundamental> mono-fundamental-get-value
	<mono:boolean> <mono:char> <mono:sbyte> <mono:byte> <mono:short> <mono:ushort> 
	<mono:int> <mono:uint> <mono:long> <mono:ulong> <mono:single> <mono:double>
	)

(define-class <mono:fundamental> (<mono:valuetype>)
  (value #:allocation #:instance #:my-init
	 (lambda (o) (%mono-object-unbox (slot-ref o 'object)))
	 #:getter mono-fundamental-get-value)
  )

(define-method (write (obj <mono:fundamental>) file)
  (display "#<" file)
  (display (class-name (class-of obj)) file)
  (display #\space file)
  (display (mono-fundamental-get-value obj) file)
  (display #\> file))

(define-class <mono:boolean> (<mono:fundamental>))
(class-slot-set! <mono:boolean> 'klass (%mono-defaults-boolean-class))

(define-class <mono:char> (<mono:fundamental>))
(class-slot-set! <mono:char> 'klass (%mono-defaults-char-class))

(define-class <mono:sbyte> (<mono:fundamental>))
(class-slot-set! <mono:sbyte> 'klass (%mono-defaults-uint8-class))

(define-class <mono:byte> (<mono:fundamental>))
(class-slot-set! <mono:byte> 'klass (%mono-defaults-int8-class))

(define-class <mono:short> (<mono:fundamental>))
(class-slot-set! <mono:short> 'klass (%mono-defaults-int16-class))

(define-class <mono:ushort> (<mono:fundamental>))
(class-slot-set! <mono:ushort> 'klass (%mono-defaults-uint16-class))

(define-class <mono:int> (<mono:fundamental>))
(class-slot-set! <mono:int> 'klass (%mono-defaults-int32-class))

(define-class <mono:uint> (<mono:fundamental>))
(class-slot-set! <mono:uint> 'klass (%mono-defaults-uint32-class))

(define-class <mono:long> (<mono:fundamental>))
(class-slot-set! <mono:long> 'klass (%mono-defaults-int64-class))

(define-class <mono:ulong> (<mono:fundamental>))
(class-slot-set! <mono:ulong> 'klass (%mono-defaults-uint64-class))

(define-class <mono:single> (<mono:fundamental>))
(class-slot-set! <mono:single> 'klass (%mono-defaults-single-class))

(define-class <mono:double> (<mono:fundamental>))
(class-slot-set! <mono:double> 'klass (%mono-defaults-double-class))

(define-class <mono:string> (<mono:fundamental>))
(class-slot-set! <mono:string> 'klass (%mono-defaults-string-class))

(define (%get-mono-class klass)
  (get-mono-class (make <mono:type> #:type (%mono-class-get-type klass))))

(%get-mono-class (%mono-defaults-multicast-delegate-class))

(define-method (initialize (obj <mono:fundamental>) . initargs)
  (let* ((instance (next-method))
	 (object (get-keyword #:object (car initargs) *unspecified*))
	 (value (get-keyword #:value (car initargs) *unspecified*))
	 (domain (get-keyword #:domain (car initargs) (mono-domain-get)))
	 (new-object (cond
		      ((not (unspecified? object))
		       object)

		      ((unspecified? value)
		       (mono-error "Missing #:value argument"))

		      ((is-a? instance <mono:boolean>) (%mono-get-boxed-boolean domain value))
		      ((is-a? instance <mono:char>) (%mono-get-boxed-char domain value))
		      ((is-a? instance <mono:sbyte>) (%mono-get-boxed-int8 domain value))
		      ((is-a? instance <mono:byte>) (%mono-get-boxed-uint8 domain value))
		      ((is-a? instance <mono:short>) (%mono-get-boxed-int16 domain value))
		      ((is-a? instance <mono:ushort>) (%mono-get-boxed-uint16 domain value))
		      ((is-a? instance <mono:int>) (%mono-get-boxed-int32 domain value))
		      ((is-a? instance <mono:uint>) (%mono-get-boxed-uint32 domain value))
		      ((is-a? instance <mono:long>) (%mono-get-boxed-uint64 domain value))
		      ((is-a? instance <mono:ulong>) (%mono-get-boxed-uint64 domain value))
		      ((is-a? instance <mono:single>) (%mono-get-boxed-float domain value))
		      ((is-a? instance <mono:double>) (%mono-get-boxed-double domain value))
		      (else (mono-error "Unknown type: ~S" (class-of instance))))))
    (slot-set! instance 'object new-object)
    instance))

(define-generic invoke)
(define-generic marshal)
(define-generic demarshal)

(define-method (marshal (type <mono:type>) (object <mono:object>))
  object)

(define-method (marshal (type <mono:type>) (object <mono:valuetype>))
  object)

(define-method (marshal (type <mono:type>) value)
  (let* ((class (get-mono-class type))
	 (object (make class #:value value)))
    object))

(define-method (demarshal (object <mono:object>))
  object)

(define-method (demarshal (object <mono:fundamental>))
  (mono-fundamental-get-value object))

(define-method (invoke (method <mono:method>) args)
  (let* ((signature (mono-method-get-signature method))
	 (params (mono-method-signature-get-param-types signature))
	 (param-count (vector-length params))
	 (static (mono-method-is-static? method))
	 (return-type (mono-method-signature-get-return-type signature))
	 (arg-vector (make-vector param-count))
	 (instance (if static *unspecified* (car args)))
	 (instance-object instance))
    (or static (set! args (cdr args)))
    (or (eq? (vector-length params) (length args))
	(mono-error "Method ~S takes ~S arguments" method param-count))
    (if (mono-method-is-static? method)
	(or (unspecified? instance)
	    (mono-error "Static method ~S doesn't take an instance argument" method))
	(begin
	  (and (unspecified? instance)
	       (mono-error "Non-static method ~S needs an instance argument" method))
	  (or (is-a? instance <mono:object>)
	      (mono-error "Invalid instance argument to method ~S: ~S" method instance))
	  (set! instance-object (slot-ref instance 'object))))
    (do ((idx 0 (+ idx 1))) ((>= idx param-count) arg-vector)
      (let* ((type (vector-ref params idx))
	     (object (marshal type (list-ref args idx)))
	     (raw-object (slot-ref object 'object)))
	(vector-set! arg-vector idx raw-object)))
    (let ((retval (%mono-runtime-invoke (slot-ref method 'method) instance-object arg-vector)))
      (if (eq? (%mono-type-get-kind (slot-ref return-type 'type)) 1)
	  *unspecified*
	  (demarshal (make (get-mono-class return-type) #:object retval))))))

(define (%do-invoke vector)
  (let* ((obj (vector-ref vector 0))
	 (klass (%get-mono-class (%mono-object-get-class obj)))
	 (instance (make klass #:object obj))
	 (args (map (lambda (aobj)
		      (let* ((aklass (%get-mono-class (%mono-object-get-class aobj))))
			(make aklass #:object aobj)))
		    (cddr (vector->list vector))))
	 (func (cdr (vector-ref vector 1)))
	 (all-args (append (list instance) args)))
    (apply func all-args)))

(define-generic mono-delegate-create)
(define-method (mono-delegate-create (type <mono:type>) (obj <mono:object>) (generic <generic>))
  (%do-create-delegate type obj generic))
(define-method (mono-delegate-create (type <mono:type>) (obj <mono:object>) (func <procedure>))
  (%do-create-delegate type obj func))

(define (%do-create-delegate type obj func)
  (let* ((klass (get-mono-class type))
	 (gw-klass (class-slot-ref klass 'klass))
	 (gw-parent (%mono-class-get-parent gw-klass))
	 (invoke (if (equal? gw-parent (%mono-defaults-multicast-delegate-class))
		     (%mono-get-delegate-invoke gw-klass)
		     (mono-error "Not a delegate class: ~S" klass)))
	 (invoke-sig (%mono-method-get-signature invoke))
	 (gw-return-type (%mono-method-signature-get-return-type invoke-sig))
	 (return-type (make <mono:type> #:type gw-return-type))
	 (target (slot-ref obj 'object))
	 (domain (%mono-object-get-domain target))
	 (invoke-func (if (eq? (%mono-type-get-kind gw-return-type) 1)
			  (lambda (vector)
			    (%do-invoke vector)
			    *unspecified*)
			  (lambda (vector)
			    (slot-ref (marshal return-type (%do-invoke vector)) 'object))))
	 (delegate (%mono-delegate-create domain (slot-ref type 'type) target invoke-func func)))
    (make klass #:object delegate)))
