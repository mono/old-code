Contents of this readme:


	1     	Building
	1.1	   Building using NAnt (http://nant.sourceforge.net)
	1.2        Building on Linux
	1.3        Building on Windows
	2	Examples
	2.1	   Logging to Console
	2.2	   Logging to File




1 Building

	1.1 Building using NAnt 

		We are now assuming that you have a working NAnt setup.

		In the root of Logger.NET, run the following command:

			nant

		If you are experiencing Logger.NET exceptions and are willing to
		report a bug, run the following command to build Logger.NET with 
		debug information which enables you to see more detailed runtime 
		information:

			nant debug

	1.2 Building on Linux

		We are now assuming you have Make installed.

		In the root of Logger.NET, run the following command:

			make

		If you are experiencing Logger.NET exceptions and are willing to
		report a bug, run the following command to build Logger.NET with 
		debug information which enables you to see more detailed runtime 
		information:

			make debug

	1.3 Building on Windows

		In the root of Logger.NET, run the following command:

			build

		If you are experiencing Logger.NET exceptions and are willing to
		report a bug, run the following command to build Logger.NET with 
		debug information which enables you to see more detailed runtime 
		information:

			build debug

2. Examples

	This section shows 2 examples. The first example logs to the console. The
	second one logs to a file.

	The following managed code is used(in C#). The app.exe.config files are 
	in the subsections below:

	using System;
	using TerWoord.Diagnostics;

	public class LogTest
	{
		public static void Main()
		{
			Category LogCategory = LogFactory.GetCategory("LogTest");
			LogCategory.EnterMethod();
			try
			{
				LogCategory.SendString("Sending some demonstration stuff...");
				LogCategory.SendValue("Milliseconds uptime", Environment.TickCount);
				LogCategory.SendError("Error Occurred");
				LogCategory.SendString("Now throwing exception");
				throw new Exception("Exception for showing Exception Sending");
			}
			catch(Exception E)
			{
				SendError(E);
				throw; // for correct stack handling.
			}
			finally
			{
				LogCategory.ExitMethod();
			}
		}
	}


	If you compile the above code to a console application, you can proceed to the
	sections 2.1 and 2.2

	2.1 Logging to Console

		This section demonstrates an app.exe.config file which lets a Logger.NET 
		enabled application log to the console.

		Below is the config file to use:

			<configuration>
				<configSections>
					<sectionGroup name="TerWoord">
						<section name="Logger.NET"
							 type="TerWoord.Configuration.LoggerNETConfigSectionHandler, Logger.NET"/>
					</sectionGroup>
				</configSections>
				<TerWoord>
					<Logger.NET>
						<Category Name="test">
							<Destinations>
								<Destination Type="TerWoord.Diagnostics.Destinations.ConsoleDestination, Logger.NET">
									<Settings> <!-- set Destination specific settings(the defaults are filled in here): -->
										<Setting Name="IndentSize" Value="2"/>
									</Settings>
								</Destination>
							</Destinations>
						</Category>
					</Logger.NET>
				</TerWoord>
			</configuration>

	2.2 Logging to a file

		This section demonstrates an app.exe.config file which lets a Logger.NET
		enabled application log to a file.

		Below is the config file to use:

			<configuration>
				<configSections>
					<sectionGroup name="TerWoord">
						<section name="Logger.NET"
							 type="TerWoord.Configuration.LoggerNETConfigSectionHandler, Logger.NET"/>
					</sectionGroup>
				</configSections>
				<TerWoord>
					<Logger.NET>
						<Category Name="test">
							<Destinations>
								<Destination Type="TerWoord.Diagnostics.Destinations.FileDestination, Logger.NET">
									<Settings> <!-- set Destination specific settings(the defaults are filled in here): -->
										<Setting Name="LogDirectory" Value="log"/>
										<Setting Name="AutoFlush" Value="true"/>
									</Settings>
								</Destination>
							</Destinations>
						</Category>
					</Logger.NET>
				</TerWoord>
			</configuration>