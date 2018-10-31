using System.Configuration.Install;
using System.ServiceProcess;

namespace PaymentRemindMonitor
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.paymentServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.monitorServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // paymentServiceProcessInstaller
            // 
            this.paymentServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.paymentServiceProcessInstaller.Password = null;
            this.paymentServiceProcessInstaller.Username = null;
            // 
            // monitorServiceInstaller
            // 
            this.monitorServiceInstaller.DisplayName = "Rent Pay Monitor";
            this.monitorServiceInstaller.ServiceName = "PaymentMonitorService";
            this.monitorServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.paymentServiceProcessInstaller,
            this.monitorServiceInstaller});
            this.Committed += new InstallEventHandler(ServiceInstaller_Committed);
        }
        void ServiceInstaller_Committed(object sender, InstallEventArgs e)
        {
            // Auto Start the Service Once Installation is Finished.
            var controller = new ServiceController("PaymentMonitorService");
            controller.Start();
        }
        #endregion

        private System.ServiceProcess.ServiceProcessInstaller paymentServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller monitorServiceInstaller;
    }
    
}