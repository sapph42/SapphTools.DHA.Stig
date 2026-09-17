using SapphTools.SecurityDescriptor.Classes;

namespace SapphTools.DHA.Stig.Remediator.Controls; 
internal class DataGridViewPrincipalCell : DataGridViewTextBoxCell {
    public override Type ValueType => typeof(Trustee);
    public override Type FormattedValueType => typeof(string);
    public override object DefaultNewRowValue => null!;
    protected override void Paint(
            Graphics graphics, 
            Rectangle clipBounds, 
            Rectangle cellBounds, 
            int rowIndex, 
            DataGridViewElementStates cellState, 
            object value, 
            object formattedValue, 
            string errorText, 
            DataGridViewCellStyle cellStyle, 
            DataGridViewAdvancedBorderStyle advancedBorderStyle, 
            DataGridViewPaintParts paintParts) {
        base.Paint(
            graphics, 
            clipBounds, 
            cellBounds, 
            rowIndex, 
            cellState, 
            value, 
            string.Empty, 
            errorText, 
            cellStyle, 
            advancedBorderStyle, 
            paintParts & ~DataGridViewPaintParts.ContentForeground);
        if (value is not Trustee principal) {
            return;
        }
        Rectangle contentBounds = BorderWidths(advancedBorderStyle);
        contentBounds = new(
            cellBounds.X + contentBounds.X + cellStyle.Padding.Left,
            cellBounds.Y + contentBounds.Y + cellStyle.Padding.Top,
            cellBounds.Width - contentBounds.Width - cellStyle.Padding.Horizontal,
            cellBounds.Height - contentBounds.Height - cellStyle.Padding.Vertical
        );
        int imageSize = Math.Min(Math.Min(principal.DisplayImage.Width, contentBounds.Height), 16);
        Rectangle imageBounds = new(
            contentBounds.X,
            contentBounds.Y + (contentBounds.Height - imageSize) / 2,
            imageSize,
            imageSize
        );
        graphics.DrawImage(principal.DisplayImage, imageBounds);
        Rectangle textBounds = new(
            imageBounds.Right + 4,
            contentBounds.Y,
            Math.Max(0, contentBounds.Right - imageBounds.Right - 4),
            contentBounds.Height
        );
        Color foreColor = (cellState & DataGridViewElementStates.Selected) != 0 ?
            cellStyle.SelectionForeColor :
            cellStyle.ForeColor;
        TextRenderer.DrawText(
            graphics,
            principal.DisplayString,
            cellStyle.Font,
            textBounds,
            foreColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine
        );
    }
    protected override Size GetPreferredSize(
            Graphics graphics, 
            DataGridViewCellStyle cellStyle, 
            int rowIndex, 
            Size constraintSize) {
        if (Value is not Trustee principal) {
            return base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);
        }
        Size textSize = TextRenderer.MeasureText(
            graphics,
            principal.DisplayString,
            cellStyle.Font,
            new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.SingleLine
        );
        int imageSize = Math.Min(Math.Min(principal.DisplayImage.Width, textSize.Height), 16);
        int width = imageSize + 4 + textSize.Width + cellStyle.Padding.Horizontal + 4;
        int height = Math.Max(imageSize, textSize.Height) + cellStyle.Padding.Vertical;
        return new Size(width, height);
    }
}
