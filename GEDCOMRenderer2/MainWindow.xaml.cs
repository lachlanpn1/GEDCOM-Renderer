using System;
using System.IO;
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
using System.Diagnostics;
using System.Windows.Shapes;
using GEDCOMLibrary;

namespace GEDCOMRenderer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double ScaleRate = 1.1;
        bool IsMouseDown;
        double xOffset;
        double yOffset;
        Point initial;
        Point endPoint = new Point(0,0);

        List<TreeMember> family = new List<TreeMember>();
        FamilyTree testFamily = new FamilyTree();
        Point firstMemberPoint = new Point(960, 0);
        double initialWidth = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Generation0LineWidth"));

        public MainWindow()
        {
            InitializeComponent();
            GenerateTreeMembers();

            Loaded += delegate
            {
                PositionTree(family[0]);
            };

        }

        private FamilyTree InitializeTestFamily()
        {
            FamilyTree familyTree = new FamilyTree();
            Person p = new Person();
            p.GivenName = "Jim";
            p.FamilyName = "Jones";
            p.Gender = Gender.Male;
            p.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Birth,
                Date = new DateTime(1900, 1, 12),
                Location = "Testville, USA",
            });
            p.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Death,
                Date = new DateTime(1981, 3, 23),
                Location = "New York, USA",
            });
            Person p2 = new Person();
            p2.GivenName = "Samantha";
            p2.FamilyName = "Jones";
            p2.MaidenName = "Wilkins";
            p2.Gender = Gender.Female;
            p2.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Birth,
                Date = new DateTime(1902, 5, 7),
                Location = "Testville, USA",
            });
            p2.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Death,
                Date = new DateTime(1983, 5, 21),
                Location = "New York, USA",
            });
            Person p3 = new Person();
            p3.GivenName = "Willie";
            p3.FamilyName = "Jones";
            p3.Gender = Gender.Male;
            p3.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Birth,
                Date = new DateTime(1935, 12, 2),
                Location = "Testville, USA",
            });
            p3.Events.Add(new GEDCOMLibrary.Event()
            {
                EventType = EventType.Death,
                Date = new DateTime(2018, 7, 15),
                Location = "New York, USA",
            });
            Family family = new Family()
            {
                Husband = p,
                Wife = p2,
            };
            family.AddChild(p3);
            p3.ChildFamily = family;
            p.SpouseFamily = family;
            p2.SpouseFamily = family;
            familyTree.Members.Add(p3);
            familyTree.Members.Add(p);
            familyTree.Members.Add(p2);
            return familyTree;
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            st.ScaleX *= ScaleRate;
            st.ScaleY *= ScaleRate;
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            st.ScaleX /= ScaleRate;
            st.ScaleY /= ScaleRate;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            initial = Mouse.GetPosition(this);
            xOffset = 0.0;
            yOffset = 0.0;
            Debug.Write("Mouse left button down \n");
            Debug.WriteLine("Initial position : " + initial);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseDown) return;
            xOffset = e.GetPosition(this).X - initial.X;
            Debug.WriteLine("xOffset: " + xOffset);
            yOffset = e.GetPosition(this).Y - initial.Y;
            Debug.WriteLine("yOffset: " + yOffset);
            tt.X = endPoint.X + xOffset;
            Debug.WriteLine("tt.X : " + tt.X);
            tt.Y = endPoint.Y + yOffset;
            Debug.WriteLine("yy.Y : " + tt.Y);

        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
            endPoint.X += xOffset;
            endPoint.Y += yOffset;
            Debug.Write("Mouse left button up\n");
            Debug.WriteLine("ENDPOINT.X = " + endPoint.X);
            Debug.WriteLine("ENDPOINT.Y = " + endPoint.Y);
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseDown = false;
            Debug.Write("Mouse leave\n");
            
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            IsMouseDown = false;
            Debug.WriteLine("Drag leave\n");
        }

        public void SetTreeMemberLocation(TreeMember treeMember, Point location)
        {
            Canvas.SetLeft(treeMember, location.X);
            Canvas.SetTop(treeMember, location.Y);
            treeMember.Location = location;
            treeMember.HasBeenPositioned = true;
        }

        // Recursive Function that positions the Family Tree
        private void PositionTreeMember(TreeMember member, Point location)
        {
            Person person = member.Person;
            // position self
            SetTreeMemberLocation(member, location);
            if(person.HasParents())
            {
                // draw parent line
                canvas.Children.Add(member.GetParentLine());
                if(person.HasFather())
                {
                    TreeMember father = FindPersonAsTreeMember(person.ChildFamily.Husband);
                    Point fatherLocation = member.GetParentLocation(father.Person.HasSpouse(), father);
                    PositionTreeMember(father, fatherLocation);
                }
                if(person.HasMother())
                {
                    TreeMember mother = FindPersonAsTreeMember(person.ChildFamily.Wife);
                    Point motherLocation = member.GetParentLocation(mother.Person.HasSpouse(), mother);
                    PositionTreeMember(mother, motherLocation);
                }
            }
            if(person.Gender == Gender.Male && person.HasSpouse())
            {
                // if male & has spouse draw spouse line
                canvas.Children.Add(member.GetSpouseLine());
            }
            // call method on children if they've still not be drawn
            if(person.HasChildren())
            {
                // not implemented.
            }
        }

        private void GenerateTreeMembers()
        {
            String file = "noonan.ged";
            GEDCOMReader gcr = new GEDCOMReader();
            StreamReader sr = new StreamReader(file);
            //FamilyTree ft = gcr.ProcessFile(sr);
            FamilyTree ft = InitializeTestFamily();

            family = GenerateTreeMember(ft.Members[0], initialWidth);

            foreach (TreeMember tm in family)
            {
                canvas.Children.Add(tm);
            }

        }

        private List<TreeMember> GenerateTreeMember(Person p, double lineWidth)
        {
            TreeMember tm = new TreeMember(p, lineWidth);
            List<TreeMember> result = new List<TreeMember>();
            result.Add(tm);

            if(p.HasParents())
            {
                if(p.HasFather())
                {
                    Person father = p.ChildFamily.Husband;
                    List<TreeMember> fatherTree = GenerateTreeMember(father, (0.400 * lineWidth));
                    result.AddRange(fatherTree);
                }
                if(p.HasMother())
                {
                    Person mother = p.ChildFamily.Wife;
                    List<TreeMember> motherTree = GenerateTreeMember(mother, (0.400 * lineWidth));
                    result.AddRange(motherTree);
                }
            }
            return result;
        }

        private void PositionTree(TreeMember firstMember)
        {
            //Queue<TreeMember> membersToPlace = new Queue<TreeMember>();
            //membersToPlace.Enqueue(firstMember);
            PositionTreeMember(firstMember, firstMemberPoint);

        }

        private TreeMember FindPersonAsTreeMember(Person person)
        {
            return (family.Find(member => member.Person == person));
        }
    }
}

