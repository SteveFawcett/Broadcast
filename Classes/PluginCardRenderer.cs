using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Broadcast.Classes
{

    public class PluginCardRenderer
    {
        private readonly Font _repoFont = new("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _zipFont = new("Segoe UI", 8, FontStyle.Regular);
        private readonly Brush _cardBrush = new SolidBrush(Color.WhiteSmoke);
        private readonly Pen _borderPen = new(Color.LightGray);
        private readonly Brush _highlightBrush = new SolidBrush(Color.LightSteelBlue);
        private readonly Brush _badgeBrush = new SolidBrush(Color.ForestGreen);
        private readonly Brush _badgeTextBrush = new SolidBrush(Color.White);
        private readonly Brush _repoTextBrush = new SolidBrush(Color.Black);
        private readonly Brush _zipTextBrush = new SolidBrush(Color.Gray);

        private const int Padding = 10;
        private const int LineHeight = 20;
        private const int CornerRadius = 8;
        private const int BadgeWidth = 70;
        private const int BadgeHeight = 22;

        public void Draw(Graphics g, Rectangle bounds, ReleaseListItem item, bool isSelected)
        {
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
   
            // Adjust bounds for spacing
            var cardRect = new Rectangle(bounds.X + 5, bounds.Y + 4, bounds.Width - 10, bounds.Height - 8);

            // Rounded card path
            using var path = CreateRoundedRect(cardRect, CornerRadius);

            // Background
            g.FillPath(_cardBrush, path);

            // Border
            g.DrawPath(_borderPen, path);

            // Selection highlight
            if (isSelected)
            {
                g.FillPath(_highlightBrush, path);
            }

            // Text layout
            int textX = cardRect.X + Padding;
            int textY = cardRect.Y + Padding;

            g.DrawString(item.Repo     , _repoFont, _repoTextBrush, textX, textY);
            g.DrawString(item.ZipName  , _zipFont , _zipTextBrush , textX, textY + LineHeight);
            if( !string.IsNullOrEmpty( item.Installed))
                g.DrawString($"Installed: {item.Installed}", _zipFont , _zipTextBrush , textX, textY + ( 2 * LineHeight));

            int badgeX = cardRect.Right - BadgeWidth - Padding;
            int badgeY = cardRect.Y + Padding;

            // Badge
            if (item.IsLatest)
            {
                DrawBadge( g , new Rectangle(badgeX, badgeY, BadgeWidth, BadgeHeight), "Latest"  , _badgeBrush , _badgeTextBrush);
            }

            badgeY += ( 6 + BadgeHeight );

            if( string.IsNullOrEmpty( item.Installed ) )
                DrawBadge(g, new Rectangle(badgeX, badgeY, BadgeWidth, BadgeHeight),
                    "Not Installed",
                    new SolidBrush(Color.ForestGreen),
                    new SolidBrush(Color.White) );
            else if (item.Installed == item.Version)
                DrawBadge(g, new Rectangle(badgeX, badgeY, BadgeWidth, BadgeHeight),
                    "Current",
                    new SolidBrush(Color.White),
                    new SolidBrush(Color.Gray));
            else
                DrawBadge(g, new Rectangle(badgeX, badgeY, BadgeWidth, BadgeHeight),
                    "Update Available",
                    new SolidBrush(Color.Blue),
                    new SolidBrush(Color.White));

        }

        private void DrawBadge(Graphics g, Rectangle irect, string text, Brush badgeBrush, Brush textBrush)
        {
            var textSize = g.MeasureString(text, _zipFont);
            Rectangle rect = irect;
            rect.Width = (int)textSize.Width + 20; // Add padding for text
            rect.X = ( irect.Left + ( irect.Width - rect.Width ) / 2) - 20; // Center the badge horizontally

            using var badgePath = CreateRoundedRect(rect, 10);
            g.FillPath(badgeBrush, badgePath);
            float textX = rect.X + (rect.Width - textSize.Width) / 2;
            float textY = rect.Y + (rect.Height - textSize.Height) / 2;

            g.DrawString(text, _zipFont, textBrush, textX, textY);
        }
        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

