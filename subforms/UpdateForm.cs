using BroadcastPluginSDK;

namespace Broadcast.subforms
{
    public partial class UpdateForm : Form
    {
        IEnumerable<Dictionary<string, string>> Plugins;
        public UpdateForm(IEnumerable<Dictionary<string, string>> plugins)
        {
            InitializeComponent();
            Plugins = plugins;
            show();
        }

        private void show()
        {
            listView1.Columns.Clear();
            listView1.Columns.Add("Name"   , 100);
            listView1.Columns.Add("File"   , 250);
            listView1.Columns.Add("Version", 50);
            listView1.Columns.Add("Description", 250);
            listView1.Columns.Add("Repository", 250);

            foreach (Dictionary<string, string> plugin in Plugins)
            {
                var item = new ListViewItem(plugin["Name"])
                {
                    Name = plugin["Name"],
                };

                item.SubItems.Add(plugin["FilePath"]);
                item.SubItems.Add(plugin["Version"]);
                item.SubItems.Add(plugin["Description"]);
                item.SubItems.Add(plugin["RepositoryUrl"]);
                listView1.Items.Add(item);
            }
        }
    }
}
