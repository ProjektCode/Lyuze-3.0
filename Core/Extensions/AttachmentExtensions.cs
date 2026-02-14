using Discord;

namespace Lyuze.Core.Extensions;

/// <summary>
/// Extension methods for working with Discord attachments.
/// </summary>
public static class AttachmentExtensions {
    
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp"];

    /// <summary>
    /// Determines whether the attachment is an image based on content type or file extension.
    /// </summary>
    /// <param name="attachment">The attachment to check.</param>
    /// <returns>True if the attachment is an image; otherwise, false.</returns>
    public static bool IsImage(this IAttachment attachment) {
        // Best case: Discord gives us a proper mime type
        if (!string.IsNullOrWhiteSpace(attachment.ContentType)) {
            if (attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Fallback: check extension
        var filename = attachment.Filename ?? "";
        return ImageExtensions.Any(ext => 
            filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
