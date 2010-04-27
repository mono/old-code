(use-modules (mono mono) (oop goops) (oop goops describe))

(define domain (mono-domain-get))
(define assembly (mono-domain-assembly-open domain "Test.exe"))

(make-mono-class (mono-assembly-find-type assembly "" "X"))
(define x (X:DoHello))

(define foo (mono-assembly-find-type assembly "" "Foo"))
(make-mono-class foo)

(define-generic Hello)
(define-method (Hello (x <X>) (a <mono:long>) (b <mono:single>))
  (display (list "HELLO" x a b)) (newline)
  (inexact->exact (* (slot-ref a 'value) (slot-ref b 'value))))

