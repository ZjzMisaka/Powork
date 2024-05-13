using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace Powork.Helper
{
    public static class ButtonHelper
    {
        public static Button CreateImageButton(string path, RoutedEventHandler handler)
        {
            Button button = new Button();
            if (!Path.Exists(path))
            {
                path = Path.GetFullPath("Image\\no_image.png");
            }
            ImageBrush imgBrush = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Absolute)))
            {
                Stretch = Stretch.Uniform
            };
            button.Background = imgBrush;
            button.Width = 128;
            button.Height = 128;
            button.Cursor = Cursors.Hand;
            button.BorderThickness = new Thickness(0);
            Style noChangeStyle = new Style(typeof(Button));
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Border));
            factory.Name = "border";
            factory.SetValue(Border.BackgroundProperty, imgBrush);
            factory.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
            template.VisualTree = factory;
            noChangeStyle.Setters.Add(new Setter(Button.TemplateProperty, template));
            noChangeStyle.Triggers.Add(new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(Button.BackgroundProperty, imgBrush) }
            });
            noChangeStyle.Triggers.Add(new Trigger
            {
                Property = Button.IsPressedProperty,
                Value = true,
                Setters = { new Setter(Button.BackgroundProperty, imgBrush) }
            });
            button.Style = noChangeStyle;
            button.Click += handler;

            return button;
        }
    }
}
