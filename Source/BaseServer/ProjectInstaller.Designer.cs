namespace Insight.Base.Server
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller
            // 
            serviceInstaller.Description = "Insight 基础服务";
            serviceInstaller.DisplayName = "Insight 基础服务";
            serviceInstaller.ServiceName = "Insight Base Service";
            serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            Installers.AddRange(new System.Configuration.Install.Installer[] {
            serviceProcessInstaller,
            serviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
    }
}