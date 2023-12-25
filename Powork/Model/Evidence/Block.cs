using CommunityToolkit.Mvvm.ComponentModel;
using Powork.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Powork.Model.Evidence
{
    public class Block : ObservableObject
    {
        private TestingPageViewModel parent;
        public Block(TestingPageViewModel parent)
        {
            this.parent = parent;
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                SetProperty<string>(ref title, value);
                parent.SheetModel = parent.SheetModel;
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                SetProperty<string>(ref description, value);
                parent.SheetModel = parent.SheetModel;
            }
        }

        private ImageSource imageSource;
        public ImageSource ImageSource
        {
            get { return imageSource; }
            set
            {
                SetProperty<ImageSource>(ref imageSource, value);
                parent.SheetModel = parent.SheetModel;
            }
        }

        private ObservableCollection<Shape> shapeList;
        public ObservableCollection<Shape> ShapeList
        {
            get { return shapeList; }
            set
            {
                SetProperty<ObservableCollection<Shape>>(ref shapeList, value);
                parent.SheetModel = parent.SheetModel;
            }
        }
    }
}
