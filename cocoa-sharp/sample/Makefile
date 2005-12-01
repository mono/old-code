all: clean GraphicsView.app View.app Cube.app Rectangle.app

GraphicsView.app: GraphicsView.exe
	macpack -n:GraphicsView -a:./GraphicsView.exe -r:NSView.nib -m:2 -o:. -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/libCocoaSharpGlue.dylib

View.app: View.exe
	macpack -n:View -a:./View.exe -r:NSView.nib -m:2 -o:. -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/libCocoaSharpGlue.dylib

Cube.app: Cube.exe
	macpack -n:Cube -a:./Cube.exe -r:NSOpenGLView.nib -m:2 -o:. -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/libCocoaSharpGlue.dylib

Rectangle.app: Rectangle.exe
	macpack -n:Rectangle -a:./Rectangle.exe -r:NSOpenGLView.nib -m:2 -o:. -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/libCocoaSharpGlue.dylib

GraphicsView.exe:
	mcs -out:GraphicsView.exe -define:SYSD NSViewSample.cs -r:System.Drawing -pkg:cocoa-sharp 

View.exe:
	mcs -out:View.exe -r:System.Drawing NSViewSample.cs -pkg:cocoa-sharp 

Rectangle.exe:
	mcs -out:Rectangle.exe -define:RECTANGLE OpenGLViewSample.cs -pkg:cocoa-sharp 

Cube.exe:
	mcs -out:Cube.exe -define:CUBE OpenGLViewSample.cs -pkg:cocoa-sharp 
	
clean:
	find . -maxdepth 1 -name '*.exe' -exec rm {} \;
	rm -Rf ./GraphicsView.app/ 
	rm -Rf ./View.app/ 
	rm -Rf ./Cube.app/ 
	rm -Rf ./Rectangle.app/ 
