using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Broadcast.SubForms
{

    public partial class UpdateForm : Form
    {
        IEnumerable<Dictionary<string, string>> Plugins;
        public UpdateForm(IEnumerable<Dictionary<string, string>> plugins)
        {
            InitializeComponent();
            Plugins = plugins;

            setup();

            show();

            foreach ( DataGridViewRow row in dataGridView1.Rows )
            {
                Fetch(  row , json => DoSomething(json));
            }
        }

        public static string ToApiRepoUrl(string repoUrl)
        {
            if (string.IsNullOrWhiteSpace(repoUrl))
                throw new ArgumentException("Repository URL cannot be null or empty.", nameof(repoUrl));

            // Normalize and parse the URL
            Uri uri = new Uri(repoUrl);
            string[] segments = uri.AbsolutePath.Trim('/').Split('/');

            if (segments.Length != 2)
                throw new FormatException("Invalid GitHub repository URL format. Expected format: https://github.com/{owner}/{repo}");

            string owner = segments[0];
            string repo = segments[1];

            return $"https://api.github.com/repos/{owner}/{repo}/";
        }
        

        public void DoSomething(JArray json)
        {
            foreach( var entry in json )
            {
                Debug.WriteLine($"{entry["tag_name"]}");
            }
            
        }
        private void Fetch(DataGridViewRow row, Action<JArray> onComplete)
        {
            var repoCell = row.Cells["Repository"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(repoCell))
                return;

            string api = ToApiRepoUrl(repoCell);
            Debug.WriteLine($"Converting for {repoCell} => {api}");

            var thread = new Thread(() =>
            {
                try
                {
                    var fetcher = new JsonFetcher(api);
                    string json = fetcher.GetJsonAsync("releases", TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                    var jArray = JArray.Parse(json);

                    string?[] tags = jArray
                        .Select(entry => entry["tag_name"]?.ToString())
                        .Where(tag => !string.IsNullOrEmpty(tag))
                        .ToArray();
                    
                    string?[] sorted = tags
                        .Select(tag => new { Original = tag, SemVer = SemVer.Parse(tag ?? String.Empty) })
                        .OrderByDescending(x => x.SemVer)
                        .Select(x => x.Original)
                        .ToArray();

                    // UI updates must be marshaled to the main thread
                    row.DataGridView?.Invoke(() =>
                    {
                        row.Cells["Options"] = new Combo(sorted );
                        ///onComplete?.Invoke(jArray);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fetch failed: {ex.Message}");
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }


        private void setup()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false; // Optional: only one row at a time

            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("Version", "Version");
            dataGridView1.Columns.Add("Repository", "Repository");
            dataGridView1.Columns.Add("Options", "Versions");

            dataGridView1.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                }
            };
        }
        private void show()
        {
            // Add ComboBox column
            dataGridView1.Rows.Clear();

            foreach (Dictionary<string, string> plugin in Plugins)
            {
                int rowIndex = dataGridView1.Rows.Add();
                var row = dataGridView1.Rows[rowIndex];

                row.Cells["Name"].Value = plugin.GetValueOrDefault("Name", "Unknown");
                row.Cells["Version"].Value = plugin.GetValueOrDefault("Version", "N/A");
                row.Cells["Repository"].Value = plugin.GetValueOrDefault("RepositoryUrl", "—");

                row.Cells["Options"] = new Combo([]);
            }

            dataGridView1.ReadOnly = false;
            dataGridView1.Columns["Name"].ReadOnly = true;
            dataGridView1.Columns["Version"].ReadOnly = true;
            dataGridView1.Columns["Repository"].ReadOnly = true;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void CloseForm(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}
