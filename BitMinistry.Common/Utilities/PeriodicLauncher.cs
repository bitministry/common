using BitMinistry.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BitMinistry.Utility
{
    public class PeriodicLauncher
    {
        Timer timer;

        private string startCommand1, startArgs1, killCommand1, killArgs1;
        private string startCommand2, startArgs2, killCommand2, killArgs2;
        int timerInterval, delayInRestart;

        private string lastCase = "";

        

        public PeriodicLauncher( Action<string> printer = null )
        {
            Screen.Print = printer ?? Screen.Print;

            Screen.Print(@"set up in app.conf/appSettings:

periodic-launcher:start-command (string)
periodic-launcher:start-args (string)

periodic-launcher:running-interval-in-seconds (int)

periodic-launcher:kill-command (string)
periodic-launcher:kill-args (string)

optional:

periodic-launcher:delay-within-restart-seconds (int)
periodic-launcher:randomize-running-interval (bool)

periodic-launcher:start-command-2 (string)
periodic-launcher:start-args-2 (string)
periodic-launcher:kill-command-2 (string)
periodic-launcher:kill-args-2 (string)

");

            startCommand1 = Config.AppSettings["periodic-launcher:start-command"];
            startArgs1 = Config.AppSettings["periodic-launcher:start-args"];
            killCommand1 = Config.AppSettings["periodic-launcher:kill-command"];
            killArgs1 = Config.AppSettings["periodic-launcher:kill-args"];

            
            startCommand2 = Config.AppSettings["periodic-launcher:start-command-2"] ?? startCommand1;
            startArgs2 = Config.AppSettings["periodic-launcher:start-args-2"] ?? startArgs1;
            killCommand2 = Config.AppSettings["periodic-launcher:kill-command-2"] ?? killCommand1;
            killArgs2 = Config.AppSettings["periodic-launcher:kill-args-2"] ?? killArgs1;


            startCommand1.ThrowIfArgumentNull("app setting: periodic-launcher:start-command");
            startArgs1.ThrowIfArgumentNull("app setting: periodic-launcher:start-args");
            killCommand1.ThrowIfArgumentNull("app setting: periodic-launcher:kill-command");
            killArgs1.ThrowIfArgumentNull("app setting: periodic-launcher:kill-args");

            timerInterval = Cnv.CInt(Config.AppSettings["periodic-launcher:running-interval-in-seconds"]) * 1000;
            if (timerInterval== 0 )
                throw new InvalidOperationException("there is no running interval (periodic-launcher:running-interval-in-seconds)");

            delayInRestart = Cnv.CInt(Config.AppSettings["periodic-launcher:delay-within-restart-seconds"]) * 1000;

            Execute(Switch.start);

            timer = new Timer();

            timer.Elapsed += Timer_Elapsed;

            ResetTimer();
        }

        

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            Execute( Switch.kill );

            Screen.Print($"sleeping {delayInRestart / 1000} seconds");
            Thread.Sleep(delayInRestart);

            lastCase = lastCase == "2" ? "" : "2";

            Execute(Switch.start);
            
            ResetTimer();
        }

        private void ResetTimer()
        {
            timer.Interval = randomizeInterval(timerInterval);
            timer.Enabled = true;
            Screen.Print($"running {timer.Interval/1000} seconds");
        }

        private Random _rnd;
        private int randomizeInterval( int smthn)
        {
            if (Config.AppSettings["periodic-launcher:randomize-running-interval"] == "true")
            {
                Screen.Print($"randomizing running interval: {smthn}");
                _rnd = _rnd ?? new Random();
                return _rnd.NextAround( smthn );
            }
            return smthn;
        }

        public enum Switch
        {
            start,
            kill
        }

        public void Execute( Switch sw  )
        {
            Screen.Print( $"Executing {sw}" );

            using ( var process = new Process() )
            {

                if (sw == Switch.start)
                {
                    process.StartInfo.FileName = lastCase == "2" ? startCommand2 : startCommand1 ;
                    process.StartInfo.Arguments = lastCase == "2" ? startArgs2 : startArgs1;
                }
                else
                {
                    process.StartInfo.FileName = lastCase == "2" ? killCommand2 : killCommand1;
                    process.StartInfo.Arguments = lastCase == "2" ? killArgs2 : killArgs1;
                }

                Screen.Print($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                
            }
            
            
        }


    }
}
