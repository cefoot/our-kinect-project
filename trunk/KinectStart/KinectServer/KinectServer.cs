using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using KinectAddons;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{
    class KinectServer
    {
        static void Main(string[] args)
        {
            PrintInfo();
            var kinect = KinectSensor.KinectSensors[0];
            var kinectAudioSource = kinect.AudioSource;
            kinect.Start();// alt Initialize(RuntimeOptions.UseSkeletalTracking);//was will ich haben
            kinect.ElevationAngle = Settings.Default.KinectAngle;//neigung
            kinect.SkeletonStream.Enable();
            var cmds = new Commands(kinect,kinectAudioSource);
            while (ReactOnCommand(cmds))
            {}




            var transferableJoints = new Skeleton().CreateTransferable();
            var serializer = new XmlSerializer(transferableJoints.GetType());
            var tmp = new JointType();
            var writer = new StringWriter();
            serializer.Serialize(writer, transferableJoints);
            writer.Flush();
            Console.WriteLine(writer.ToString());
            Console.ReadKey();
        }

        private static void PrintInfo()
        {
            Console.Clear();
            Console.WriteLine(String.Concat("Kinect Server: ", Assembly.GetAssembly(typeof (KinectServer)).GetName().Version));
            Console.WriteLine("                          ::::::        ");
            Console.WriteLine("                     :::::::::::::::7   ");
            Console.WriteLine("                +::::::::::::::::::::   ");
            Console.WriteLine("           7::::::::,::::::::,::::::::  ");
            Console.WriteLine("       ::::::::::::::::::::::::::~+,~?7 ");
            Console.WriteLine("   :::::::::::::~:::::::::::,~+,,??,=?  ");
            Console.WriteLine("  ~~::::::,:::::::::::::,=?I,???7??7+?  ");
            Console.WriteLine("  =?=~,~:::::::::::::~I?,~?? ??? ??  +  ");
            Console.WriteLine(" I~=====~~:::::~:,I??,I?  ?? 7?   ?  +  ");
            Console.WriteLine(" I  ::~===,=?,~?I,?II ??  +   ?   ?  +7 ");
            Console.WriteLine(" ?   + :~=:??,+?? I?+  I  =   ?   ?     ");
            Console.WriteLine(" ?         I? +??  I   ?   ?  ?         ");
            Console.WriteLine(" 7         I?  I   ?   ?   ?            ");
            Console.WriteLine("            ?  I   I   ?                ");
            Console.WriteLine("            I  I   I                    ");
            Console.WriteLine("            ?   7                       ");
        }

        private static bool ReactOnCommand(Commands cmds)
        {
            var command = Console.ReadLine();
            var methodInfo = cmds.GetType().GetMethods().FirstOrDefault(method => method.Name.ToLower().Equals(command.ToLower()));
            //var methodInfo = cmds.GetType().GetMethod(command, BindingFlags.IgnoreCase);
            if(methodInfo != null && methodInfo.GetParameters().Length == 0)
            {
                return (bool)methodInfo.Invoke(cmds, null);
            }
            Console.Clear();
            PrintInfo();
            Console.WriteLine("Command '{0}' not found", command);
            return true;
        }

        private static void RuntimeSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
