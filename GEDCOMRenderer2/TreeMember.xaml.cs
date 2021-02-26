using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GEDCOMLibrary;

namespace GEDCOMRenderer2
{
    /// <summary>
    /// Interaction logic for TreeMember.xaml
    /// </summary>
    public partial class TreeMember : UserControl
    {
        double CHILD_LINE_LENGTH = Convert.ToDouble(ConfigurationManager.AppSettings.Get("ChildLineLength"));

        public Point Location { get; set; }
        public double SpouseLineWidth { get; set; }
        public double ChildLineXOffset { get; set; }
        public double ParentLineXOffset { get; set; }
        public double ChildLineWidth { get; set; }
        public double ParentLineWidth { get; set; }
        public bool HasBeenPositioned { get; set; } = false;

        public Person Person { get; set; }

        public TreeMember(Person person)
        {
            InitializeComponent();
            this.Person = person;
            TreeMemberName.Text = this.Person.GetDisplayName(true);
            TreeMemberDate.Text = this.Person.GetLifeDatesAsString();
        }

        public TreeMember(Person person, double lineWidth)
        {
            InitializeComponent();
            double generation = GetGeneration(lineWidth);
            this.Person = person;
            TreeMemberName.Text = this.Person.GetDisplayName(true);
            TreeMemberDate.Text = this.Person.GetLifeDatesAsString();


            double TreeMemberNameFontSize = CalculateTreeMemberNameFontSize(generation);
            double TreeMemberDateFontSize = CalculateTreeMemberDateFontSize(generation);

            TreeMemberName.FontSize = TreeMemberNameFontSize;
            TreeMemberDate.FontSize = TreeMemberDateFontSize;

            SpouseLineWidth = lineWidth;
            ChildLineXOffset = 0.5 * lineWidth;
            ParentLineXOffset = ChildLineXOffset;
            ChildLineWidth = CHILD_LINE_LENGTH;
            ParentLineWidth = ChildLineWidth;
        }

        public Point GetRightAnchor()
        {
            double anchorX = Location.X + this.ActualWidth;
            double anchorY = Location.Y + (this.ActualHeight / 2);
            return new Point(anchorX, anchorY);
        }

        public Point GetLeftAnchor()
        {
            double anchorX = Location.X;
            double anchorY = Location.Y + (this.ActualHeight / 2);
            return new Point(anchorX, anchorY);
        }

        public Point GetTopAnchor()
        {
            double anchorX = Location.X + (this.ActualWidth / 2);
            double anchorY = Location.Y;
            return new Point(anchorX, anchorY);
        }

        public Point GetBottomAnchor()
        {
            double anchorX = Location.X + (this.ActualWidth * 0.5);
            double anchorY = Location.Y + this.ActualHeight;
            return new Point(anchorX, anchorY);
        }

        public Line GetChildLine(bool hasSpouse)
        {
            Line line = new Line();
            line.StrokeThickness = Convert.ToDouble(ConfigurationManager.AppSettings.Get("ChildLineThickness"));
            line.Stroke = (Brush)Application.Current.Resources["ChildLineBrush"];
            // MaleLocation.
            if (Person.Gender == Gender.Male)
            {
                line.X1 = hasSpouse ? GetRightAnchor().X + ChildLineXOffset : Location.X + (0.5 * this.ActualWidth);
                line.Y1 = hasSpouse ? Location.Y + (0.5 * this.ActualHeight) : Location.Y + this.ActualHeight;
                line.X2 = line.X1;
                line.Y2 = Location.Y + ChildLineWidth;
            }
            // Female
            if (Person.Gender == Gender.Female)
            {
                line.X1 = hasSpouse ? GetRightAnchor().X - ChildLineXOffset : Location.X + (0.5 * this.ActualWidth);
                line.Y1 = hasSpouse ? Location.Y + (0.5 * this.ActualHeight) : Location.Y + this.ActualHeight;
                line.X2 = line.X1;
                line.Y2 = Location.Y + ChildLineWidth;
            }
            return line;
        }

        public Line GetParentLine()
        {
            Line line = new Line();
            line.StrokeThickness = Convert.ToDouble(ConfigurationManager.AppSettings.Get("ParentLineThickness"));
            line.Stroke = (Brush)Application.Current.Resources["ParentLineBrush"];
            Point bottomAnchor = GetBottomAnchor();
            line.X1 = bottomAnchor.X;
            line.Y1 = bottomAnchor.Y;
            line.X2 = line.X1;
            line.Y2 = line.Y1 + ParentLineWidth - (0.5 * this.ActualHeight);
            return line;
        }

        public Line GetSpouseLine()
        {
            Line line = new Line();
            line.StrokeThickness = Convert.ToDouble(ConfigurationManager.AppSettings.Get("SpouseLineThickness"));
            line.Stroke = (Brush)Application.Current.Resources["SpouseLineBrush"];
            // Male
            if(Person.Gender == Gender.Male)
            {
                line.X1 = GetRightAnchor().X;
                line.Y1 = GetRightAnchor().Y;
                line.X2 = GetRightAnchor().X + SpouseLineWidth;
                line.Y2 = line.Y1;
            }
            // Female
            if (Person.Gender == Gender.Female)
            {
                line.X1 = GetLeftAnchor().X;
                line.Y1 = GetLeftAnchor().Y;
                line.X2 = GetLeftAnchor().X - SpouseLineWidth;
                line.Y2 = line.Y1;
            }
            return line;
        }

        public Point GetSpouseLocation()
        {
            Point SpouseLocation = new Point();

            if(Person.Gender == Gender.Male)
            {
                SpouseLocation.X = Location.X + this.ActualWidth + SpouseLineWidth;
                SpouseLocation.Y = Location.Y;
            }
            else
            {
                SpouseLocation.X = Location.X - this.ActualWidth - SpouseLineWidth;
                SpouseLocation.Y = Location.Y;
            }
            return SpouseLocation;
        }

        public Point GetChildLocation(bool hasSpouse)
        {
            Point ChildLocation = new Point();
            
            if(Person.Gender == Gender.Male)
            {
                if(hasSpouse)
                {
                    ChildLocation.X = (Location.X + this.ActualWidth + ChildLineXOffset) - (0.5 * this.ActualWidth);
                    ChildLocation.Y = Location.Y + ChildLineWidth;
                }
                else
                {
                    ChildLocation.X = Location.X;
                    ChildLocation.Y = Location.Y + ChildLineWidth;
                }
            } 
            else
            { 
                ChildLocation.X = (Location.X - this.ActualWidth - ChildLineXOffset) + (0.5 * this.ActualWidth);
                ChildLocation.Y = Location.Y + ChildLineWidth;
            }
            return ChildLocation;
        }

        public Point GetParentLocation(bool parentHasSpouse, TreeMember parent)
        {
            Point ParentLocation = new Point();
            if (parentHasSpouse)
            {
                if(parent.Person.Gender == Gender.Male)
                {
                    ParentLocation.X = (Location.X + (0.5 * this.ActualWidth)) - (0.5 * parent.SpouseLineWidth) - parent.ActualWidth;
                    ParentLocation.Y = Location.Y + ParentLineWidth;
                }
                else
                {
                    ParentLocation.X = (Location.X + (0.5 * this.ActualWidth)) + (0.5 * parent.SpouseLineWidth);
                    ParentLocation.Y = Location.Y + ParentLineWidth;
                }
            }
            else
            {
                ParentLocation.X = Location.X;
                ParentLocation.Y = Location.Y + ParentLineWidth;
            }

            return ParentLocation;
        }

        private double GetGeneration(double lineWidth)
        {
            double maxWidth = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Generation0LineWidth"));
            double maxWidthDividedByCurrentWidth = maxWidth / lineWidth;
            return Math.Log(maxWidthDividedByCurrentWidth, 2);
        }

        private double CalculateTreeMemberNameFontSize(double generation)
        {
            double initialFontSize = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TreeMemberNameFontSize"));
            if (generation == 0)
            {
                return initialFontSize;
            }
            return initialFontSize / generation;
        }

        private double CalculateTreeMemberDateFontSize(double generation)
        {
            double initialFontSize = Convert.ToDouble(ConfigurationManager.AppSettings.Get("TreeMemberDateFontSize"));
            if(generation == 0)
            {
                return initialFontSize;
            }
            return initialFontSize / generation;
        }
    }
}
