using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Editor
{
    public partial class MainWindow : Window
    {
        private Model3DGroup objectGroup = new Model3DGroup();
        private Model3DGroup lightingGroup = new Model3DGroup();
        public int listBoxSize = -1;
        private FramesCollection framesCollection = new FramesCollection();
        private ArrayList scaleArray = new ArrayList();
        private int selectedIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            selectedIndex++;

            CreateWorld();
            InstantiateSliders();
            UpdateList();

            listBoxSize++;

            ClearFrameList(0);

            scaleArray.Add(1.0);
        }

        public void ClearFrameList(int index)
        {
            var framesList = framesCollection.childFrameList.ElementAt(index);

            if (framesList.Count() < framesSlider.Maximum)
            for (int i = 0; i < framesSlider.Maximum; i++)
            {
                framesList.Add(new Frame{keyFrame =  -1});
            }
            else
            {
                for (int i = 0; i < framesSlider.Maximum; i++)
                {
                    framesList[i] = new Frame {keyFrame = -1};
                }
            }
        }

        private Point3DCollection Vertex(StreamReader sr, int offset = 0)
        {
            Point3DCollection vertex = new Point3DCollection();
            while (!sr.EndOfStream)
            {
                String data = sr.ReadLine();

                if (data.Length > 0 && data.ElementAt(0) == 'v' && data.ElementAt(1) == ' ')
                {
                    int lastLook = 1;

                    double x = 0, y = 0, z = 0;

                    int numRead = 0;

                    for (var i = 2; i < data.Length; i++)
                    {
                        if (data.ElementAt(i) == ' ' || i == data.Length - 1)
                        {
                            if (i + 1 < data.Length)
                            if (data.ElementAt(i) == ' ')
                            {
                                while (data.ElementAt(i) == ' ')
                                {
                                    i++;
                                }
                            }

                            int length;
                            if (i == data.Length - 1)
                                length = i - lastLook + 1;
                            else
                                length = i - lastLook;
                            
                            switch (numRead)
                            {
                                case 0:
                                    x = Convert.ToDouble(data.Substring(lastLook, length)) + offset;
                                    break;
                                case 1:
                                    y = Convert.ToDouble(data.Substring(lastLook, length));
                                    break;
                                case 2:
                                    z = Convert.ToDouble(data.Substring(lastLook, length));
                                    break;
                            }

                            numRead++;
                            lastLook = i;

                            if (numRead == 3)
                            {
                                Point3D copy = new Point3D(x, y, z);
                                vertex.Add(copy);
                            }
                        }
                    }
                }
            }
            return vertex;
        }

        private Int32Collection VertexIndicies(StreamReader sr)
        {
            Int32Collection indices = new Int32Collection();

            while (!sr.EndOfStream)
            {
                String data = sr.ReadLine();

                if (data.Length > 0 && data.ElementAt(0) == 'f' && data.ElementAt(1) == ' ')
                {
                    int lastLook = 1;
                    int x = 0, y = 0, z = 0, w = 0;

                    int numRead = 0;

                    for (int i = 2; i < data.Length; i++)
                    {
                        int length = 0;
                        if (i == data.Length - 1)
                            length = i - lastLook + 1;
                        else
                            length = i - lastLook;

                        if (data.ElementAt(i) == '/')
                        {
                            switch (numRead)
                            {
                                case 0:
                                    x = Convert.ToInt32(data.Substring(lastLook, length));
                                    break;
                                case 1:
                                    y = Convert.ToInt32(data.Substring(lastLook, length));
                                    break;
                                case 2:
                                    z = Convert.ToInt32(data.Substring(lastLook, length));
                                    break;
                                case 3:
                                    w = Convert.ToInt32(data.Substring(lastLook, length));
                                    break;
                            }

                            while (data.ElementAt(i) != ' ' && i != data.Length - 1)
                            {
                                i++;
                            }

                            numRead++;
                            lastLook = i;
                        }

                        if (numRead == 4)
                        {
                            indices.Add(x - 1);
                            indices.Add(y - 1);
                            indices.Add(w - 1);
                            indices.Add(y - 1);
                            indices.Add(z - 1);
                            indices.Add(w - 1);
                        }
                        else if (i == data.Length - 1 && numRead == 3)
                        {
                            indices.Add(x - 1);
                            indices.Add(y - 1);
                            indices.Add(z - 1);
                        }
                    }
                }
            }

            return indices;
        }

        private void ObjLoader(string filePath, MeshGeometry3D mesh, int offset = 0)
        {
            StreamReader sr = new StreamReader(filePath);
            StreamReader sr2 = new StreamReader(filePath);
            
            mesh.Positions = Vertex(sr, offset);
            mesh.TriangleIndices = VertexIndicies(sr2);
        }

        private void CreateWorld()
        {
            ModelVisual3D objectMv3D = new ModelVisual3D();
            ModelVisual3D lightingMv3D = new ModelVisual3D();

            // Declare scene object
            PerspectiveCamera mainCam = new PerspectiveCamera();
            mainCam.Position = new Point3D(0, 5, 5);
            mainCam.LookDirection = new Vector3D(0, -5, -5);
            mainCam.FieldOfView = 75;
            vp3D.Camera = mainCam;

            DirectionalLight directionalLight = new DirectionalLight();
            directionalLight.Direction = new Vector3D(-1, -1, -0.5);
            directionalLight.Color = Colors.GhostWhite;

            lightingGroup.Children.Add(directionalLight);
            lightingMv3D.Content = lightingGroup;

            DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            SpecularMaterial specularMaterial = new SpecularMaterial();
            diffuseMaterial.Brush = Brushes.Chocolate;
            //diffuseMaterial.Color = Colors.WhiteSmoke;
            specularMaterial.Brush = Brushes.Aquamarine;

            MeshGeometry3D newSphere = new MeshGeometry3D();
            ObjLoader("sphere.obj", newSphere);

            GeometryModel3D newMesh = new GeometryModel3D();
            newMesh.Material = diffuseMaterial;
            newMesh.Geometry = newSphere;

            newMesh.BackMaterial = specularMaterial;

            Transform3D sphereTransform = new TranslateTransform3D(new Vector3D(-2, -2, 0));
            newMesh.Transform = sphereTransform;

            objectGroup.Children.Add(newMesh);
            
            objectMv3D.Content = objectGroup;

            vp3D.Children.Add(objectMv3D);
            vp3D.Children.Add(lightingMv3D);
        }

        private void InstantiateSliders()
        {
            if (selectedIndex == -1)
            {
                redSlider.IsEnabled = false;
                blueSlider.IsEnabled = false;
                greenSlider.IsEnabled = false;
                xPositionSlider.IsEnabled = false;
                yPositionSlider.IsEnabled = false;
                zPositionSlider.IsEnabled = false;
                framesSlider.IsEnabled = false;
                Save_Frame.IsEnabled = false;
            }
        }

        private void UpdateSliders()
        {
            if (selectedIndex <= objectGroup.Children.Count && selectedIndex > -1)
            {
                GeometryModel3D tempModel = objectGroup.Children.ElementAt(selectedIndex) as GeometryModel3D;

                xPositionSlider.Value = tempModel.Transform.Value.OffsetX;
                yPositionSlider.Value = tempModel.Transform.Value.OffsetY;
                zPositionSlider.Value = tempModel.Transform.Value.OffsetZ;

                scaleSlider.Value = (double)scaleArray[selectedIndex];

                DiffuseMaterial currentDiffuseColour = tempModel.Material as DiffuseMaterial;

                redSlider.Value = (currentDiffuseColour.Brush as SolidColorBrush).Color.R;
                greenSlider.Value = (currentDiffuseColour.Brush as SolidColorBrush).Color.G;
                blueSlider.Value = (currentDiffuseColour.Brush as SolidColorBrush).Color.B;
            }
        }

        private void blueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            blueSlider.IsEnabled = selectedIndex != -1;

            if (objectListBox.SelectedIndex > -1)
            {
                GeometryModel3D currentGeometryModel3D = objectGroup.Children[selectedIndex] as GeometryModel3D;

                DiffuseMaterial currentColor = currentGeometryModel3D.Material as DiffuseMaterial;

                SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((currentColor.Brush as SolidColorBrush).Color.R, (currentColor.Brush as SolidColorBrush).Color.G, (byte)blueSlider.Value));
                Color colour = Color.FromRgb(currentColor.Color.R, currentColor.Color.G, currentColor.Color.B);

                DiffuseMaterial dm = new DiffuseMaterial
                {
                    Brush = brush,
                    Color = colour
                };

                if (currentGeometryModel3D != null)
                    currentGeometryModel3D.Material = dm;
            }
        }

        private void redSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            redSlider.IsEnabled = selectedIndex != -1;

            if (objectListBox.SelectedIndex <= -1)
                return;

            GeometryModel3D currentGeometryModel3D = objectGroup.Children[selectedIndex] as GeometryModel3D;

            DiffuseMaterial currentColor = currentGeometryModel3D.Material as DiffuseMaterial;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)redSlider.Value, (currentColor.Brush as SolidColorBrush).Color.G, (currentColor.Brush as SolidColorBrush).Color.B));
            Color colour = Color.FromRgb(currentColor.Color.R, currentColor.Color.G, currentColor.Color.B);

            DiffuseMaterial dm = new DiffuseMaterial
            {
                Brush = brush,
                Color = colour
            };

            currentGeometryModel3D.Material = dm;
        }

        private void greenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            greenSlider.IsEnabled = selectedIndex != -1;

            if (objectListBox.SelectedIndex <= -1)
                return;

            GeometryModel3D currentGeometryModel3D = objectGroup.Children[selectedIndex] as GeometryModel3D;

            DiffuseMaterial currentColor = currentGeometryModel3D.Material as DiffuseMaterial;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((currentColor.Brush as SolidColorBrush).Color.R,
                (byte) greenSlider.Value, (currentColor.Brush as SolidColorBrush).Color.B));
            Color colour = Color.FromRgb(currentColor.Color.R, currentColor.Color.G, currentColor.Color.B);

            DiffuseMaterial dm = new DiffuseMaterial
            {
                Brush = brush,
                Color = colour
            };

            currentGeometryModel3D.Material = dm;
        }

        private void Add_Sphere_Click(object sender, RoutedEventArgs e)
        {
            MeshGeometry3D newSphere = new MeshGeometry3D();
            GeometryModel3D newMesh = new GeometryModel3D();
            DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            SpecularMaterial specularMaterial = new SpecularMaterial();

            diffuseMaterial.Brush = Brushes.GhostWhite;

            int count = 0;
            foreach (var sphere in objectGroup.Children)
            {
                if (sphere != null && ((GeometryModel3D)sphere).BackMaterial == null)
                    count++;
            }

            if (count == objectGroup.Children.Count)
                specularMaterial.Brush = Brushes.Aquamarine;

            ObjLoader("sphere.obj", newSphere);

            newMesh.Material = diffuseMaterial;

            if (specularMaterial.Brush != null)
                newMesh.BackMaterial = specularMaterial;

            newMesh.Geometry = newSphere;

            objectGroup.Children.Add(newMesh);

            framesCollection.childFrameList.Add(new FramesList());

            scaleArray.Add(1.0);

            listBoxSize++;
            ClearFrameList(listBoxSize);

            UpdateList();
        }

        private void Remove_Sphere_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIndex > -1)
                objectGroup.Children.RemoveAt(selectedIndex);

            scaleArray.Clear();

            listBoxSize--;

            for (int i = 0; i < objectGroup.Children.Count; i++)
            {
                scaleArray.Add(1.0);
            }

            framesCollection.childFrameList.RemoveAt(selectedIndex);

            UpdateList();
        }

        public void UpdateList()
        {
            List<String> geoList = new List<String>();

            for (int i = 0; i < objectGroup.Children.Count; i++)
                geoList.Add("Sphere " + (char) (i + 65));

            objectListBox.ItemsSource = geoList;
        }

        private void xPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            xPositionSlider.IsEnabled = selectedIndex > -1;

            if (!xPositionSlider.IsEnabled)
                return;

            GeometryModel3D currentModel3D = (GeometryModel3D) objectGroup.Children.ElementAt(selectedIndex);

            Transform3D newSphereTransform = new TranslateTransform3D(xPositionSlider.Value, currentModel3D.Transform.Value.OffsetY, currentModel3D.Transform.Value.OffsetZ);

            var incrementScale = framesCollection.childFrameList.ElementAt(selectedIndex).framesList
                .ElementAt((int) framesSlider.Value - 1).scale;

            ScaleTransform3D scale = new ScaleTransform3D(incrementScale.ScaleX, incrementScale.ScaleY, incrementScale.ScaleZ);

            Transform3DGroup tg = new Transform3DGroup();

            tg.Children.Add(scale);
            tg.Children.Add(newSphereTransform);

            currentModel3D.Transform = tg;
        }

        private void yPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            yPositionSlider.IsEnabled = selectedIndex > -1;

            if (!yPositionSlider.IsEnabled)
                return;

            GeometryModel3D currentModel3D = objectGroup.Children.ElementAt(selectedIndex) as GeometryModel3D;

            Transform3D newSphereTransform = new TranslateTransform3D(currentModel3D.Transform.Value.OffsetX, yPositionSlider.Value, currentModel3D.Transform.Value.OffsetZ);

            var incrementScale = framesCollection.childFrameList.ElementAt(selectedIndex).framesList
                .ElementAt((int)framesSlider.Value - 1).scale;

            ScaleTransform3D scale = new ScaleTransform3D(incrementScale.ScaleX, incrementScale.ScaleY, incrementScale.ScaleZ);

            Transform3DGroup tg = new Transform3DGroup();

            tg.Children.Add(scale);
            tg.Children.Add(newSphereTransform);

            currentModel3D.Transform = tg;
        }

        private void zPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            zPositionSlider.IsEnabled = selectedIndex > -1;

            if (!zPositionSlider.IsEnabled)
                return;

            GeometryModel3D currentModel3D = (GeometryModel3D)objectGroup.Children.ElementAt(selectedIndex);
            Transform3D newSphereTransform = new TranslateTransform3D(currentModel3D.Transform.Value.OffsetX, currentModel3D.Transform.Value.OffsetY, zPositionSlider.Value);

            var incrementScale = framesCollection.childFrameList.ElementAt(selectedIndex).framesList
                .ElementAt((int)framesSlider.Value - 1).scale;

            ScaleTransform3D scale = new ScaleTransform3D(incrementScale.ScaleX, incrementScale.ScaleY, incrementScale.ScaleZ);

            Transform3DGroup tg = new Transform3DGroup();

            tg.Children.Add(scale);
            tg.Children.Add(newSphereTransform);

            currentModel3D.Transform = tg;
        }

        private void scaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleSlider.IsEnabled = selectedIndex > -1;

            if (!scaleSlider.IsEnabled)
                return;

            GeometryModel3D currentSphere = objectGroup.Children.ElementAt(selectedIndex) as GeometryModel3D;
            Transform3D newScale = new ScaleTransform3D(scaleSlider.Value, scaleSlider.Value, scaleSlider.Value);

            Transform3D translate = new TranslateTransform3D(currentSphere.Transform.Value.OffsetX, currentSphere.Transform.Value.OffsetY, currentSphere.Transform.Value.OffsetZ);

            Transform3DGroup tGroup = new Transform3DGroup();

            tGroup.Children.Add(newScale);
            tGroup.Children.Add(translate);
            currentSphere.Transform = tGroup;

            scaleArray.Insert(selectedIndex, scaleSlider.Value);
            scaleArray.RemoveAt(selectedIndex + 1);
        }

        private void objectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = objectListBox.SelectedIndex;

            Save_Frame.IsEnabled = true;
            framesSlider.IsEnabled = true;

            xPositionSlider.IsEnabled = true;
            yPositionSlider.IsEnabled = true;
            zPositionSlider.IsEnabled = true;

            scaleSlider.IsEnabled = true;

            if (selectedIndex <= -1)
                return;
            
            GeometryModel3D currentSphere = objectGroup.Children.ElementAt(selectedIndex) as GeometryModel3D;

            var framesList = framesCollection.childFrameList.ElementAt(selectedIndex);

            for (int i = (int)framesSlider.Value - 1; i < framesList.Count(); i++)
            {
                if (framesList[i].keyFrame != -1)
                    continue;

                var frame = framesList[i];
                frame.transform = currentSphere.Transform;
                frame.scale = new ScaleTransform3D((double)scaleArray[selectedIndex], (double)scaleArray[selectedIndex], (double)scaleArray[selectedIndex]);
                frame.brush = ((DiffuseMaterial)currentSphere.Material).Brush as SolidColorBrush;
                frame.colour = ((DiffuseMaterial)currentSphere.Material).Color;
                framesList[i] = frame;
            }

            for (int i = 0; i < objectGroup.Children.Count; i++)
            {
                if (scaleArray.Count < objectGroup.Children.Count)
                    scaleArray.Add(1.0);
            }

            UpdateSliders();
        }

        private void Save_Frame_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIndex <= -1)
                return;

            GeometryModel3D currentSphere = objectGroup.Children.ElementAt(selectedIndex) as GeometryModel3D;

            double offsetX = currentSphere.Transform.Value.OffsetX;
            double offsetY = currentSphere.Transform.Value.OffsetY;
            double offsetZ = currentSphere.Transform.Value.OffsetZ;

            Transform3D framePosition = new TranslateTransform3D(offsetX, offsetY, offsetZ);

            var framesList = framesCollection.childFrameList.ElementAt(selectedIndex);

            var frames = framesList[(int) framesSlider.Value - 1];
            frames.keyFrame = (int) framesSlider.Value - 1;
            frames.transform = framePosition;
            frames.scale = new ScaleTransform3D((double)scaleArray[selectedIndex], (double)scaleArray[selectedIndex], (double)scaleArray[selectedIndex]);
            frames.brush = ((DiffuseMaterial) currentSphere.Material).Brush as SolidColorBrush;
            frames.colour = ((DiffuseMaterial) currentSphere.Material).Color;

            framesList[(int) framesSlider.Value - 1] = frames;
        }

        private void FramesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (selectedIndex <= -1)
            {
                framesSlider.IsEnabled = false;
                return;
            }

            for (int i = 0; i < framesCollection.childFrameList.Count; i++)
            {
                GeometryModel3D currentSphere = objectGroup.Children.ElementAt(i) as GeometryModel3D;
                int frameNumber = (int) framesSlider.Value - 1;
                var framesList = framesCollection.childFrameList.ElementAt(i);
                Frame currentKeyFrame = framesList[frameNumber];

                DiffuseMaterial dm = currentSphere.Material as DiffuseMaterial;
                if (currentKeyFrame.keyFrame != -1)
                {
                    Transform3DGroup tg = new Transform3DGroup();

                    tg.Children.Add(currentKeyFrame.scale);
                    tg.Children.Add(currentKeyFrame.transform);

                    currentSphere.Transform = tg;

                    dm = new DiffuseMaterial
                    {
                        Brush = currentKeyFrame.brush,
                        Color = currentKeyFrame.colour
                    };

                    currentSphere.Material = dm;

                    UpdateSliders();
                    return;
                }

                while (framesList[frameNumber].keyFrame == -1 && frameNumber < framesList.Count() - 1)
                {
                    frameNumber++;
                }

                if (frameNumber == (int) framesSlider.Value - 1)
                    return;

                var nextFrame = framesList[frameNumber];
                frameNumber = (int) framesSlider.Value - 1;

                while (framesList[frameNumber].keyFrame == -1 && frameNumber >= framesSlider.Minimum)
                {
                    frameNumber--;
                }

                if (frameNumber == 0)
                    return;

                var prevFrame = framesList[frameNumber];
                var relativeChange = framesSlider.Value - prevFrame.keyFrame;
                var deltaFrames = nextFrame.keyFrame - prevFrame.keyFrame;

                var frames = framesList[(int) framesSlider.Value - 1];

                if (framesSlider.Value <= nextFrame.keyFrame && framesSlider.Value > prevFrame.keyFrame)
                {
                    frames.transform = Interpolate(nextFrame.transform, prevFrame.transform, deltaFrames,
                        (int) relativeChange);
                    frames.scale = (ScaleTransform3D) Interpolate(nextFrame.scale, prevFrame.scale, deltaFrames,
                        (int) relativeChange);
                    frames.brush = Interpolate(nextFrame.brush.Color, prevFrame.brush.Color, deltaFrames,
                        (int) relativeChange);

                    framesList[(int) framesSlider.Value - 1] = frames;

                    dm.Brush = frames.brush;

                    Transform3DGroup tg = new Transform3DGroup();

                    tg.Children.Add(frames.scale);
                    tg.Children.Add(frames.transform);

                    currentSphere.Transform = tg;
                    currentSphere.Material = dm;
                }
            }

            UpdateSliders();
        }

        public Transform3D Interpolate(Transform3D goal, Transform3D prev, int deltaFrames, int relativeChange)
        {
            double xDifference = goal.Value.OffsetX - prev.Value.OffsetX;
            double yDifference = goal.Value.OffsetY - prev.Value.OffsetY;
            double zDifference = goal.Value.OffsetZ - prev.Value.OffsetZ;

            var incrementX = prev.Value.OffsetX + (xDifference * relativeChange) / deltaFrames;
            var incrementY = prev.Value.OffsetY + (yDifference * relativeChange) / deltaFrames;
            var incrementZ = prev.Value.OffsetZ + (zDifference * relativeChange) / deltaFrames;

            return new TranslateTransform3D(incrementX, incrementY, incrementZ);
        }

        public SolidColorBrush Interpolate(Color goal, Color prev, int deltaFrames, int relativeChange)
        {
            int redDifference = goal.R - prev.R;
            int greenDifference = goal.G - prev.G;
            int blueDifference = goal.B - prev.B;

            var incrementRed = prev.R + (redDifference * relativeChange) / deltaFrames;
            var incrementGreen = prev.G + (greenDifference * relativeChange)/ deltaFrames;
            var incrementBlue = prev.B + (blueDifference * relativeChange)/ deltaFrames;

            if (incrementRed < 0)
                incrementRed += 255;
            else if (incrementRed > 255)
                incrementRed -= 255;

            if (incrementGreen < 0)
                incrementGreen += 255;
            else if (incrementGreen > 255)
                incrementGreen -= 255;

            if (incrementBlue < 0)
                incrementBlue += 255;
            else if (incrementBlue > 255)
                incrementBlue -= 255;

            return new SolidColorBrush(Color.FromRgb((byte)incrementRed, (byte) incrementGreen, (byte) incrementBlue));
        }

        public Transform3D Interpolate(ScaleTransform3D goal, ScaleTransform3D prev, int deltaFrames, int relativeChange)
        {
            double xDifference = goal.ScaleX - prev.ScaleX;
            double yDifference = goal.ScaleY - prev.ScaleY;
            double zDifference = goal.ScaleZ - prev.ScaleZ;

            var incrementX = prev.ScaleX + (xDifference * relativeChange) / deltaFrames;
            var incrementY = prev.ScaleY + (yDifference * relativeChange) / deltaFrames;
            var incrementZ = prev.ScaleZ + (zDifference * relativeChange) / deltaFrames;

            return new ScaleTransform3D(incrementX, incrementY, incrementZ);
        }

        private void Clear_Frames_Click(object sender, RoutedEventArgs e)
        {
            ClearFrameList(selectedIndex);
        }
    }
}

