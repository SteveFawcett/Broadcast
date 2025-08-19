using Broadcast.Classes;
using BroadcastPluginSDK.Interfaces;
using Markdig;
using System;
using System.Diagnostics;
using System.Security.Policy;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Broadcast.SubForms;

public partial class UpdateForm : Form
{
    private IReadOnlyList<IPlugin> _plugins;
    private const string jsonUrl = "https://raw.githubusercontent.com/SteveFawcett/delivery/refs/heads/main/releases.json";

    private async void UpdateForm_Load(object sender, EventArgs e)
    {
        await webView21.EnsureCoreWebView2Async();

        var service = await ReleaseService.CreateAsync(jsonUrl);
        var releases = service.GetReleaseItems();

        bool first = true;
        foreach (var release in releases)
        {
            if (first)
            {
                string html = await GetReadme(release.ReadMeUrl);
                webView21.NavigateToString(html);
                first = false;
            }

            listBox1.Items.Add(release);
        }
    }

    private async Task<string> GetReadme( string  url )
    {
        try
        {
            string markdown = await StringDownloader.DownloadStringAsync(url) ?? "xx";
            string html = Markdown.ToHtml(markdown);
            return html;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching README: {ex.Message}");
            return string.Empty;
        }
    }
    private async void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listBox1.SelectedItem is ReleaseListItem selected)
        {
            string markdown = await GetReadme(selected.ReadMeUrl);

            Debug.WriteLine($"Repo: {selected.Repo}\nVersion: {selected.Version}\nZip: {selected.ZipName}");

            if (!string.IsNullOrWhiteSpace(markdown))
            {
                webView21.NavigateToString(markdown);
            }
            else
            {
                webView21.NavigateToString("<p>README not found or failed to load.</p>");
            }
        }
    }

    public UpdateForm(IReadOnlyList<IPlugin> plugins)
    {
        _plugins = plugins;

        InitializeComponent();
        listBox1.AutoSize = false;

        listBox1.DrawMode = DrawMode.OwnerDrawFixed;
        listBox1.ItemHeight = 60; // Adjust for visual space
        listBox1.DrawItem += ListBox1_DrawItem;
        listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        this.Load += UpdateForm_Load;

        listBox1.Dock = DockStyle.Fill;
        listBox1.Margin = new Padding(0);
        listBox1.AutoSize = false;
        listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        //Setup();
    }
    private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var item = (ReleaseListItem)listBox1.Items[e.Index];
        var g = e.Graphics;
        var bounds = e.Bounds;

        // Background
        g.FillRectangle(new SolidBrush(e.BackColor), bounds);

        // Repo + Version
        var font = new Font("Segoe UI", 10, FontStyle.Bold);
        g.DrawString($"{item.Repo} - {item.Version}", font, Brushes.Black, bounds.X + 5, bounds.Y + 5);

        // Zip name
        var subFont = new Font("Segoe UI", 9, FontStyle.Regular);
        g.DrawString(item.ZipName, subFont, Brushes.Gray, bounds.X + 5, bounds.Y + 25);

        // Latest badge
        if (item.IsLatest)
        {
            var badge = new Rectangle(bounds.Right - 80, bounds.Y + 5, 70, 20);
            g.FillRectangle(Brushes.Green, badge);
            g.DrawString("Latest", subFont, Brushes.White, badge.X + 10, badge.Y + 2);
        }

        e.DrawFocusRectangle();
    }
    private void CloseForm(object sender, MouseEventArgs e)
    {
        Close();
    }

    private void webView21_Click(object sender, EventArgs e)
    {

    }
}