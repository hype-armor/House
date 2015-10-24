namespace EchoServer
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
            this.echoServerProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.echoServerInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // echoServerProcessInstaller1
            // 
            this.echoServerProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.echoServerProcessInstaller1.Password = null;
            this.echoServerProcessInstaller1.Username = null;
            // 
            // echoServerInstaller1
            // 
            this.echoServerInstaller1.DisplayName = "EchoServer";
            this.echoServerInstaller1.ServiceName = "EchoServer";
            this.echoServerInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.echoServerProcessInstaller1,
            this.echoServerInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller echoServerProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller echoServerInstaller1;
    }
}