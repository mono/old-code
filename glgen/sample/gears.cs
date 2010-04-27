namespace GLDemo {
    using System;
    using System.Runtime.InteropServices;
    using gl;

    public class GearsDemo {

	private static double rotx = 20.0;                       // View's X-Axis Rotation
	private static double roty = 30.0;                       // View's Y-Axis Rotation
	private static double rotz = 0.0;                        // View's Z-Axis Rotation
	private static uint gear1;                               // Display List For Red Gear
	private static uint gear2;                               // Display List For Green Gear
	private static uint gear3;                               // Display List For Blue Gear
	private static float rAngle = 0.0f;                      // Rotation Angle
	private static float[] pos = {5.0f, 5.0f, 10.0f, 0.0f};  // Light Position
	private static float[] red = {0.8f, 0.1f, 0.0f, 1.0f};   // Red Material
	private static float[] green = {0.0f, 0.8f, 0.2f, 1.0f}; // Green Material
	private static float[] blue = {0.2f, 0.2f, 1.0f, 1.0f};  // Blue Material

	private static int time = Environment.TickCount;
	private static int frame = 0;

	public static int Main (string[] args)
	{
	    glut.Init(args.Length, args);
	    glut.InitDisplayMode(glut.GLUT_RGB | glut.GLUT_DOUBLE | glut.GLUT_DEPTH);
	    glut.InitWindowSize(300, 300);
	    int window = glut.CreateWindow("OpenGL#");

	    glut.DisplayFunc(new glut.VoidCB(mydisp));
	    glut.KeyboardFunc(new glut.KeyCB(key));
	    glut.ReshapeFunc(new glut.IntIntCB(reshape));
	    glut.IdleFunc(new glut.VoidCB(idle));

	    MakeGears();

	    gl.Enable(gl.GL_CULL_FACE);
	    gl.Enable(gl.GL_LIGHTING);
	    gl.Lightfv(gl.GL_LIGHT0, gl.GL_POSITION, pos);
	    gl.Enable(gl.GL_LIGHT0);
	    gl.Enable(gl.GL_DEPTH_TEST);

	    gl.Enable(gl.GL_NORMALIZE);
	    glut.MainLoop();
	    return 0;
	}

	public static void mydisp ()
	{
		gl.Clear(gl.GL_COLOR_BUFFER_BIT | gl.GL_DEPTH_BUFFER_BIT);

		gl.PushMatrix();
		gl.Rotated(rotx, 1.0, 0.0, 0.0);                 // Position The World
		gl.Rotated(roty, 0.0, 1.0, 0.0);
		gl.Rotated(rotz, 0.0, 0.0, 1.0);

		gl.PushMatrix();
		gl.Translated(-3.0, -2.0, 0.0);                  // Position The Red Gear
		gl.Rotatef(rAngle, 0.0f, 0.0f, 1.0f);            // Rotate The Red Gear
		gl.Color3f(0.8f, 0.2f, 0.0f);
		gl.CallList(gear1);                              // Draw The Red Gear
		gl.PopMatrix();

		gl.PushMatrix();
		gl.Translated(3.1, -2.0, 0.0);                   // Position The Green Gear
		gl.Rotated(-2.0 * rAngle - 9.0, 0.0, 0.0, 1.0);  // Rotate The Green Gear
		gl.Color3f(0.0f, 0.8f, 0.2f);
		gl.CallList(gear2);                              // Draw The Green Gear
		gl.PopMatrix();

		gl.PushMatrix();
		gl.Translated(-3.1, 4.2, 0.0);                   // Position The Blue Gear
		gl.Rotated(-2.0 * rAngle - 25.0, 0.0, 0.0, 1.0); // Rotate The Blue Gear
		gl.Color3f(0.2f, 0.2f, 1.0f);
		gl.CallList(gear3);                              // Draw The Blue Gear
		gl.PopMatrix();
		gl.PopMatrix();

		rAngle += 0.2f;                                 // Increase The Rotation

		glut.SwapBuffers();

		float tmp = (Environment.TickCount - time) / 1000.0f;
		float fps = frame / tmp;
		if (tmp >= 5) {
		    Console.Write("FPS: {0}\n", fps);
		    time = Environment.TickCount;
		    frame = 0;
		}
		frame++;
	}

	public static void key (byte c, int x, int y)
	{
		//Console.WriteLine(c);
		switch (c) {
		case 27:
			Console.WriteLine("quit");
			break;
		}
	}

	public static void reshape (int width, int height)
	{

	    float h = (float) height / (float) width;

	    gl.Viewport(0, 0, width,  height);
	    gl.MatrixMode(gl.GL_PROJECTION);
	    gl.LoadIdentity();
	    gl.Frustum(-1.0, 1.0, -h, h, 5.0, 60.0);
	    gl.MatrixMode(gl.GL_MODELVIEW);
	    gl.LoadIdentity();
	    gl.Translatef(0.0f, 0.0f, -40.0f);
	}

	public static void idle ()
	{
	    glut.PostRedisplay();
	}
	private static void MakeGear(double inner_radius, double outer_radius, double width, int teeth, double tooth_depth) {
	    int i;
	    double r0;
	    double r1;
	    double r2;
	    double angle;
	    double da;
	    double u;
	    double v;
	    double len;

	    r0 = inner_radius;
	    r1 = outer_radius - tooth_depth / 2.0;
	    r2 = outer_radius + tooth_depth / 2.0;

	    da = 2.0 * Math.PI / teeth / 4.0;
	    gl.ShadeModel(gl.GL_FLAT);

	    gl.Normal3d(0.0, 0.0, 1.0);

	    /* draw front face */
	    gl.Begin(gl.GL_QUAD_STRIP);
	    for(i = 0; i <= teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), width * 0.5);
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 * Math.Sin(angle + 3 * da), width * 0.5);
	    }
	    gl.End();

	    /* draw front sides of teeth */
	    gl.Begin(gl.GL_QUADS);
	    da = 2.0 * Math.PI / teeth / 4.0;
	    for(i = 0; i < teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;

		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + da), r2 * Math.Sin(angle + da), width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + 2 * da), r2 * Math.Sin(angle + 2 * da), width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 *Math.Sin(angle + 3 * da), width * 0.5);
	    }
	    gl.End();

	    gl.Normal3d(0.0, 0.0, -1.0);

	    /* draw back face */
	    gl.Begin(gl.GL_QUAD_STRIP);
	    for(i = 0; i <= teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;
		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), -width * 0.5);
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), -width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 * Math.Sin(angle + 3 * da), -width * 0.5);
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), -width * 0.5);
	    }
	    gl.End();

	    /* draw back sides of teeth */
	    gl.Begin(gl.GL_QUADS);
	    da = 2.0 * Math.PI / teeth / 4.0;
	    for(i = 0; i < teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;

		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 * Math.Sin(angle + 3 * da), -width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + 2 * da), r2 * Math.Sin(angle + 2 * da), -width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + da), r2 * Math.Sin(angle + da), -width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), -width * 0.5);
	    }
	    gl.End();

	    /* draw outward faces of teeth */
	    gl.Begin(gl.GL_QUAD_STRIP);
	    for(i = 0; i < teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;

		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle), r1 * Math.Sin(angle), -width * 0.5);
		u = r2 * Math.Cos(angle + da) - r1 * Math.Cos(angle);
		v = r2 * Math.Sin(angle + da) - r1 * Math.Sin(angle);
		len = Math.Sqrt(u * u + v * v);
		u /= len;
		v /= len;
		gl.Normal3d(v, -u, 0.0);
		gl.Vertex3d(r2 * Math.Cos(angle + da), r2 * Math.Sin(angle + da), width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + da), r2 * Math.Sin(angle + da), -width * 0.5);
		gl.Normal3d(Math.Cos(angle), Math.Sin(angle), 0.0);
		gl.Vertex3d(r2 * Math.Cos(angle + 2 * da), r2 * Math.Sin(angle + 2 * da), width * 0.5);
		gl.Vertex3d(r2 * Math.Cos(angle + 2 * da), r2 * Math.Sin(angle + 2 * da), -width * 0.5);
		u = r1 * Math.Cos(angle + 3 * da) - r2 * Math.Cos(angle + 2 * da);
		v = r1 * Math.Sin(angle + 3 * da) - r2 * Math.Sin(angle + 2 * da);
		gl.Normal3d(v, -u, 0.0);
		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 * Math.Sin(angle + 3 * da), width * 0.5);
		gl.Vertex3d(r1 * Math.Cos(angle + 3 * da), r1 * Math.Sin(angle + 3 * da), -width * 0.5);
		gl.Normal3d(Math.Cos(angle), Math.Sin(angle), 0.0);
	    }

	    gl.Vertex3d(r1 * Math.Cos(0), r1 * Math.Sin(0), width * 0.5);
	    gl.Vertex3d(r1 * Math.Cos(0), r1 * Math.Sin(0), -width * 0.5);

	    gl.End();

	    gl.ShadeModel(gl.GL_SMOOTH);

	    /* draw inside radius cylinder */
	    gl.Begin(gl.GL_QUAD_STRIP);
	    for(i = 0; i <= teeth; i++) {
		angle = i * 2.0 * Math.PI / teeth;
		gl.Normal3d(-Math.Cos(angle), -Math.Sin(angle), 0.0);
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), -width * 0.5);
		gl.Vertex3d(r0 * Math.Cos(angle), r0 * Math.Sin(angle), width * 0.5);
	    }
	    gl.End();
	}

	private static void MakeGears() {
	    // Make The Gears
	    gear1 = gl.GenLists(1);                                                  // Generate A Display List For The Red Gear
	    gl.NewList(gear1, gl.GL_COMPILE);                                           // Create The Display List
	    gl.Materialfv(gl.GL_FRONT, gl.GL_AMBIENT_AND_DIFFUSE, red);                    // Create A Red Material
	    gl.Color3f(0.8f, 0.2f, 0.0f);
	    MakeGear(1.0, 4.0, 1.0, 20, 0.7);                                       // Make The Gear
	    gl.EndList();                                                            // Done Building The Red Gear's Display List

	    gear2 = gl.GenLists(1);                                                  // Generate A Display List For The Green Gear
	    gl.NewList(gear2, gl.GL_COMPILE);                                           // Create The Display List
	    gl.Materialfv(gl.GL_FRONT, gl.GL_AMBIENT_AND_DIFFUSE, green);                  // Create A Green Material
	    gl.Color3f(0.0f, 0.8f, 0.2f);
	    MakeGear(0.5, 2.0, 2.0, 10, 0.7);                                       // Make The Gear
	    gl.EndList();                                                            // Done Building The Green Gear's Display List

	    gear3 = gl.GenLists(1);                                                  // Generate A Display List For The Blue Gear
	    gl.NewList(gear3, gl.GL_COMPILE);                                           // Create The Display List
	    gl.Materialfv(gl.GL_FRONT, gl.GL_AMBIENT_AND_DIFFUSE, blue);                   // Create A Blue Material
	    gl.Color3f(0.2f, 0.2f, 1.0f);
	    MakeGear(1.3, 2.0, 0.5, 10, 0.7);                                       // Make The Gear
	    gl.EndList();                                                            // Done Building The Blue Gear's Display List
	}

    }
}
