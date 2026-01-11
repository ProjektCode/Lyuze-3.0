using System;
using System.Drawing;

namespace Lyuze.Core.Shared.Images.Layouts {

    public sealed class BannerLayout {
        // Baseline you designed around
        private const float BaseW = 1100f;
        private const float BaseH = 450f;

        public int Width { get; }
        public int Height { get; }
        public float Scale { get; }

        // Panel
        public RectangleF PanelRect { get; }
        public float PanelCornerRadius { get; }

        // Avatar
        public int AvatarSize { get; }
        public float AvatarBorderThickness { get; }
        public PointF AvatarTopLeft { get; }

        // Text
        public float HeadlineMaxFont { get; }
        public float HeadlineMinFont { get; }
        public float SubtextMinFont { get; }
        public float TextMaxWidth { get; }
        public PointF TextStart { get; }

        public BannerLayout(int width, int height) {
            Width = width;
            Height = height;

            // Scale based on smaller dimension to keep proportions stable
            Scale = MathF.Min(width / BaseW, height / BaseH);

            // Panel sizing (centered "card")
            float panelW = width * 0.70f;
            float panelH = height * 0.55f;
            float panelX = (width - panelW) / 2f;
            float panelY = (height - panelH) / 2f;

            PanelRect = new RectangleF(panelX, panelY, panelW, panelH);
            PanelCornerRadius = 22f * Scale;

            // Avatar sizing (relative to panel)
            AvatarSize = (int)MathF.Round(panelH * 0.40f); // ~40% of panel height
            AvatarBorderThickness = MathF.Max(3f, 6f * Scale);

            // Avatar position: centered, slightly toward top of panel
            float avatarX = panelX + (panelW - AvatarSize) / 2f;
            float avatarY = panelY + panelH * 0.12f;
            AvatarTopLeft = new PointF(avatarX, avatarY);

            // Text sizing rules
            HeadlineMaxFont = 36f * Scale;
            HeadlineMinFont = 18f * Scale;
            SubtextMinFont = 12f * Scale;

            // Text area max width is panel width minus padding
            float panelPadding = panelW * 0.08f;
            TextMaxWidth = panelW - panelPadding * 2f;

            // Text starts below avatar with spacing
            float textY = avatarY + AvatarSize + panelH * 0.08f;
            TextStart = new PointF(panelX + panelPadding, textY);
        }
    }
}
