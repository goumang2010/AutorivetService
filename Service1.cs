using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using GoumangToolKit;


namespace AutorivetService
{
    public partial class Service1 : ServiceBase
    {

      SocketServerService localServer = new SocketServerService();
        public Service1()
        {
            InitializeComponent();
            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 1000 *28800;
            t.Elapsed += new System.Timers.ElapsedEventHandler(RunWork);
            t.AutoReset = true;//设置是执行一次（false),还是一直执行（true);
            t.Enabled = true;//是否执行
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //Start the socket server
                localServer.btnBeginListen();
            }
            catch (Exception ee)
            {

            }


        }

        protected override void OnStop()
        {

        }
        public async void RunWork(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
          
            //检查配置文件
            string routinepath = @"F:\Prepare\SOFTWARE_PUB\Autorivet_team_manage\settings\PartDBRoutine";
            //检查配置文件
            var PartRoutine = jsonMethod.ReadFromFile(routinepath);
            var lastrocord = PartRoutine.Last();
            var lastDate = DateTime.Parse(lastrocord.date);
            var span = DateTime.Now.Date - lastDate;
            //If the span is so long, then update the database
            if (span.Days > 7)
            {
                //Get the update path
                var up = GoumangToolKit.localMethod.GetConfigValue("UPDATE_PATH", "PartDBCfg.py");
               var col = GoumangToolKit.localMethod.GetConfigValue("PART_MONGO_COLNAME", "PartDBCfg.py");
                PartDB pd = new PartDB(col);
                foreach (dynamic pp in up)
                {
                  await  pd.UpdatePartDB((string)pp);

                }

                var ftpup= GoumangToolKit.localMethod.GetConfigValue("FTP_UPDATE_PATH", "PartDBCfg.py");
                var ftpcol = GoumangToolKit.localMethod.GetConfigValue("FTP_PART_MONGO_COLNAME", "PartDBCfg.py");
                var ftppd= new PartDB(ftpcol);
                 foreach (dynamic pp in ftpup)
                   {
                        await ftppd.UpdateFTPPartDB((string)pp);

                   }


                }

            PartRoutine.Add(new historyJsonModel()
            {
                writer = "System",
                date = DateTime.Now.ToShortDateString(),
                operation = "Done"

            });
         PartRoutine.WriteToFile(routinepath);
            }
            catch
            {
                //Do nothing
            }

        }
    }
}
