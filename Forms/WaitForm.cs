using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using System.Diagnostics;

namespace Broadcast.SubForms
{
    public partial class WaitForm : Form
    {
        private List<IManager> _managers;
        public WaitForm(List<IManager> managers)
        {
            _managers = managers;
            InitializeComponent();
        }

        private void btnForce_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CheckLocks(object sender, EventArgs e)
        {
            foreach (var manager in _managers)
            {
                if (manager.Locked )
                {
                    if( manager is IPlugin plugin)
                        Debug.WriteLine($"Plugin {plugin.ShortName} is still locked.");

                    return;
                }
            }
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
