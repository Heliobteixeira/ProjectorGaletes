using System;
using Gwen;
using Gwen.Skin;
using Gwen.Control;
using Gwen.Control.Layout;
using System.Collections.Generic;


namespace ProjectorGaletes
{

    public class GUI : DockBase
    {

        private PancakeCoil _coil;
        public PancakeCoil coil
        {
            set { 
                _coil = value;
                LoadCoilData(_coil);
            }
        }
        public float scale
        {
            set { sliderScale.Value = value;}
            get { return (float)sliderScale.Value;}
        }

        public bool positioningActive = false;
        private int _decX;
        private int _decY;
        private float _zoom;
        private float _rotation;
        private int _offsetTurn;

        private float _plcAngle;
        private int _plcTurn;
        private int _plcDirection;


        private Gwen.Control.Base m_LastControl;
        private readonly StatusBar m_StatusBar;
        private readonly ListBox m_TextOutput;
        private TabButton m_Button;
        private readonly CollapsibleList bob_List;
        private readonly CollapsibleList def_List;
        private readonly Center m_Center;
        private readonly LabeledCheckBox m_DebugCheck;

        public double Fps; // set this in your rendering loop
        public string Note; // additional text to display in status bar

        private int rowHeight = 23;
        private int colWidth;

        private GLGrid gridPos = new GLGrid();
                    
        private GLLabel lblOrdem;
        private GLTextBox tbOrdem;
        private GLButton btCarregar;
        private GLLabel lblGal;
        private GLComboBox cbCoils;

        private GLButton btTogPositioning;
        private GLLabel gridLabels;
        private GLLabel lblPositionX;
        private GLLabel lblPositionY;
        private GLLabel lblZoom;
        private GLLabel lblRotation;
        private GLLabel lblOffsetTurn;

        GLGrid gridPLCStateLabels;
        private GLLabel lblCoilAngle;
        private GLLabel lblCoilTurn;
        private GLLabel lblCoilDirection;

        //private GLUDScale nUDScale;
        private GLHorizontalSlider sliderScale;
        private GLPropertyTree propsPancakeCoil;
        private Gwen.Control.Properties mandrelProps;
        private Gwen.Control.Properties coilProps;

        private changedSelectedCoilDelegate coilChangedCallback;
        private changedWindingDelegate projectChangedCallback;
        private changedScaleDelegate scaleChangedCallback;
        private togglePositionDelegate toggledPositioningCallback;

        public delegate void changedSelectedCoilDelegate (int coilNbr);
        public delegate void changedWindingDelegate(string project);
        public delegate void changedScaleDelegate(float scale);
        public delegate void togglePositionDelegate(bool toggledPositioningOn);


        public int decX
        {
            get { return _decX; }
            set
            {
                _decX = value;
                updatePositioningLabels();
            }
        }

        public int decY
        {
            get { return _decY; }
            set
            {
                _decY = value;
                updatePositioningLabels();
            }
        }

        public float zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                updatePositioningLabels();
            }
        }

        public float rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                updatePositioningLabels();
            }
        }

        public int offsetTurn
        {
            get { return _offsetTurn; }
            set
            {
                _offsetTurn = value;
                updatePositioningLabels();
            }
        }

        public float plcAngle
        {
            get { return _plcAngle; }
            set
            {
                _plcAngle = value;
                updatePositioningLabels();
            }
        }

        public int plcTurn
        {
            get { return _plcTurn; }
            set
            {
                _plcTurn = value;
                updatePositioningLabels();
            }
        }

        public int plcDirection
        {
            get { return _plcDirection; }
            set
            {
                _plcDirection = value;
                updatePositioningLabels();
            }
        }

        public GUI(Gwen.Control.Base parent, changedSelectedCoilDelegate coilChangedCallback, changedWindingDelegate projectChangedCallback, changedScaleDelegate scaleChangedCallback, togglePositionDelegate toggledPositioningCallback)
            : base(parent)
        {

            this.coilChangedCallback = coilChangedCallback;
            this.projectChangedCallback = projectChangedCallback;
            this.scaleChangedCallback = scaleChangedCallback;
            this.toggledPositioningCallback = toggledPositioningCallback;

            Dock = Pos.Fill;
            SetSize(parent.Width, parent.Height);
     
            bob_List = new CollapsibleList(this);
            def_List = new CollapsibleList(this);

            LeftDock.TabControl.AddPage("Bobinagem", bob_List);
            LeftDock.TabControl.AddPage("Definições", def_List);
            LeftDock.Width = 200;

            m_TextOutput = new ListBox(BottomDock);
            m_Button = BottomDock.TabControl.AddPage("Output", m_TextOutput);
            BottomDock.Height = 100;

            m_DebugCheck = new LabeledCheckBox(bob_List);
            m_DebugCheck.Text = "Mostrar dimensões";
            m_DebugCheck.CheckChanged += DebugCheckChanged;

            m_StatusBar = new StatusBar(this);
            m_StatusBar.Dock = Pos.Bottom;
            m_StatusBar.AddControl(m_DebugCheck, true);

            m_Center = new Center(this);
            m_Center.Dock = Pos.Fill;

            GUnit test;

            GLGrid gridPos = new GLGrid();

            
            lblOrdem = new GLLabel(bob_List, "Ordem:");
            tbOrdem = new GLTextBox(bob_List, "E1320100A");
            tbOrdem.Width = 90;
            //tbOrdem.TextChanged += OnProjectSelect;
            btCarregar = new GLButton(bob_List, "Ok");
            btCarregar.Clicked += OnProjectSelect;

            gridPos.AddChild(0, 0, lblOrdem);
            gridPos.AddChild(0, 1, tbOrdem);
            gridPos.AddChild(0, 2, btCarregar);


            lblGal = new GLLabel(bob_List, "Galete:");
            cbCoils = new GLComboBox(bob_List);
            cbCoils.Width = 90;
            cbCoils.ItemSelected += OnCoilSelect;

            gridPos.AddChild(2, 0, lblGal);
            gridPos.AddChild(2, 1, cbCoils);

            //GLGrid gridSettings = new GLGrid();
            /*
            lblScale = new GLLabel(def_List, "Escala:");
            sliderScale = new GLHorizontalSlider(def_List);
            sliderScale.SetSize(120, 20);
            sliderScale.Value = 50;
            sliderScale.ValueChanged += OnScaleChange;
            */
            btTogPositioning = new GLButton(def_List, "Ajustar Posição");
            btTogPositioning.IsToggle = true;
            btTogPositioning.ToggledOn += onTogglePositioningOn;
            btTogPositioning.ToggledOff += onTogglePositioningOff;

            //nUDScale = new GLUDScale(def_List);
            //nUDScale.ValueChanged += OnScaleChange;

            //gridSettings.AddChild(0, 0, lblScale);
            //gridSettings.AddChild(0, 1, sliderScale);

            gridPos.Redraw();


            createPLCStateLabels();
            createPositionLabels();

            m_StatusBar.SendToBack();
            PrintText("Unit Test started!");
        }

        private int PosRow(int row)
        {
            return row * rowHeight;
        }

        private int PosCol(int col)
        {
            return col * colWidth;
        }

        public void RegisterUnitTest(string name, CollapsibleCategory cat, GUnit test)
        {
            Button btn = cat.Add(name);
            test.Dock = Pos.Fill;
            test.Hide();
            test.UnitTest = this;
            btn.UserData = test;
            btn.Clicked += OnCategorySelect;
        }

        private void DebugCheckChanged(Gwen.Control.Base control, EventArgs args)
        {
            if (m_DebugCheck.IsChecked)
                m_Center.DrawDebugOutlines = true;
            else
                m_Center.DrawDebugOutlines = false;
            Invalidate();
        }

        private void OnCategorySelect(Gwen.Control.Base control, EventArgs args)
        {
            if (m_LastControl != null)
            {
                m_LastControl.Hide();
            }
            Gwen.Control.Base test = control.UserData as Gwen.Control.Base;
            test.Show();
            m_LastControl = test;
        }

        public void setScale(float value)
        {
            sliderScale.Value = value;
        }

        public void PrintText(string str)
        {
            m_TextOutput.AddRow(str);
            m_TextOutput.ScrollToBottom();
        }

        protected override void Layout(Gwen.Skin.Base skin)
        {
            base.Layout(skin);
        }

        protected override void Render(Gwen.Skin.Base skin)
        {
            m_StatusBar.Text = String.Format("GWEN.Net Unit Test - {0:F0} fps. {1}", Fps, Note);

            base.Render(skin);
        }

        public void fillUpAvailableCoilsComboBox(List<int> listCoilsNbrs) {
            listCoilsNbrs.Sort();
            cbCoils.DeleteAll();
            cbCoils.ItemSelected -= OnCoilSelect;
            foreach(int nbr in listCoilsNbrs)
            {
                cbCoils.AddItem("Galete #" + nbr, UserData: nbr);
            }
            cbCoils.ItemSelected += OnCoilSelect;

        }

        void OnCoilSelect(Gwen.Control.Base control, EventArgs args)
        {
            ComboBox combo = control as ComboBox;
            // UnitPrint(String.Format("ComboBox: OnComboSelect: {0}", combo.SelectedItem.Text));
            int coilNbr = (int) combo.SelectedItem.UserData;
            coilChangedCallback(coilNbr);
        }

        void OnProjectSelect(Gwen.Control.Base control, EventArgs args)
        {
            string project = tbOrdem.Text;
            projectChangedCallback(project);
        }

        void OnScaleChange(Gwen.Control.Base control, EventArgs args)
        {
            float scaleValue = sliderScale.Value;
            scaleChangedCallback(scaleValue);
        }

        private void onTogglePositioningOn(Gwen.Control.Base control, EventArgs args)
        {
            lblPositionX.Show();
            lblPositionY.Show();
            lblRotation.Show();
            lblZoom.Show();
            lblOffsetTurn.Show();
                        
            toggledPositioningCallback(true);
            //UnitPrint("Button: OnToggleOn");
        }

        private void updatePositioningLabels()
        {
            lblPositionX.Text = String.Format("Posição X: {0}", _decX);
            lblPositionY.Text = String.Format("Posição Y: {0}", _decY);
            lblZoom.Text = String.Format("Zoom: {0}", _zoom.ToString("0.0000"));
            lblRotation.Text = String.Format("Rotacão: {0}", _rotation.ToString("0.0"));
            lblOffsetTurn.Text = String.Format("Offset Espira: {0}", _offsetTurn.ToString("0"));
        }

        private void createPLCStateLabels()
        {
            gridPLCStateLabels = new GLGrid(LeftDock.Width + 10, this.Height-BottomDock.Height-m_StatusBar.Height-50);

            lblCoilAngle = new GLLabel(this, "Ângulo Mesa:", 0, 0);
            lblCoilTurn = new GLLabel(this, "Espiras:", 0, 0);
            lblCoilDirection = new GLLabel(this, "Direcção", 0, 0);

            gridPLCStateLabels.AddChild(0, 0, lblCoilAngle);
            gridPLCStateLabels.AddChild(1, 0, lblCoilTurn);
            gridPLCStateLabels.AddChild(2, 0, lblCoilDirection);

            //updatePositioningLabels(); // Needs to write numbers to Labels before updating their position
            gridPLCStateLabels.Redraw();
        }

        private void createPositionLabels()
        {
            // Inicializar com string "" aquando criação. Depois de inserir o primeiro valor faz update e actualiza labels correctamente
            GLGrid gridLabels = new GLGrid(LeftDock.Width + 10, 5);
            lblPositionX = new GLLabel(this, "Posição X:", 0, 0);
            lblPositionY = new GLLabel(this, "Posição Y:", 0, 0);
            lblZoom = new GLLabel(this, "Zoom:", 0, 0);
            lblRotation = new GLLabel(this, "Rotação", 0, 0);
            lblOffsetTurn = new GLLabel(this, "Offset Espira", 0, 0);

            lblPositionX.Hide();
            lblPositionY.Hide();
            lblZoom.Hide();
            lblRotation.Hide();
            lblOffsetTurn.Hide();

            gridLabels.AddChild(0, 0, lblPositionX);
            gridLabels.AddChild(1, 0, lblPositionY);
            gridLabels.AddChild(2, 0, lblZoom);
            gridLabels.AddChild(3, 0, lblRotation);
            gridLabels.AddChild(4, 0, lblOffsetTurn);

            gridLabels.Redraw();
        }

        public void repositionGridLabels()
        {
            gridPLCStateLabels.originX = LeftDock.Width + 10;
            gridPLCStateLabels.originY = this.Height - BottomDock.Height - m_StatusBar.Height - 50;
            gridPLCStateLabels.Redraw();
            //gridLabels.Redraw();
        }

        private void onTogglePositioningOff(Gwen.Control.Base control, EventArgs args)
        {
            lblPositionX.Hide();
            lblPositionY.Hide();
            lblZoom.Hide();
            lblRotation.Hide();
            lblOffsetTurn.Hide();
            toggledPositioningCallback(false);
            //UnitPrint("Button: ToggledOff");
        }

        public void LoadCoilData(PancakeCoil coil)
        {
            CoilMandrel mandrel = coil.mandrel;


            propsPancakeCoil = new GLPropertyTree(bob_List, 0, 400);
            propsPancakeCoil.SetBounds(0, 45, 180, 300);

            mandrelProps = propsPancakeCoil.Add("Mandril");
            mandrelProps.Add("Valid", mandrel.isValid.ToString());
            mandrelProps.Add("C", mandrel.C.ToString());
            mandrelProps.Add("E", mandrel.E.ToString());
            mandrelProps.Add("Ri", mandrel.Ri.ToString());
            mandrelProps.Add("HR", mandrel.HR.ToString());
            mandrelProps.Add("H", mandrel.H.ToString());
            mandrelProps.Add("F", mandrel.F.ToString());

            coilProps = propsPancakeCoil.Add("Galete");
            coilProps.Add("Nr", coil.Nr.ToString());
            coilProps.Add("nrEsp", coil.nrEsp.ToString());
            coilProps.Add("H", coil.H.ToString());
            coilProps.Add("dimRadFx", coil.dimRadFx.ToString());
            coilProps.Add("nrFx", coil.nrFx.ToString());
            coilProps.Add("K", coil.K.ToString());
            coilProps.Add("A", coil.A.ToString());
            coilProps.Add("B", coil.B.ToString());
            coilProps.Add("C", coil.C.ToString());
            coilProps.Add("W", coil.W.ToString());
            coilProps.Add("G", coil.G.ToString());
            coilProps.Add("E", coil.E.ToString());
            coilProps.Add("Ri", coil.Ri.ToString());
            coilProps.Add("Re", coil.Re.ToString());                    
            coilProps.Add("saidaAouT", coil.saidaAouT.ToString());
            coilProps.Add("sentido", coil.sentido.ToString());
            coilProps.Add("Hmed_A", coil.Hmed_A.ToString());
            coilProps.Add("Hmed_A1", coil.Hmed_A1.ToString());
            coilProps.Add("Hmed_A2", coil.Hmed_A2.ToString());
            coilProps.Add("Hmed_G", coil.Hmed_G.ToString());
            coilProps.Add("Hmed_B", coil.Hmed_B.ToString());
            coilProps.Add("Hmed_R", coil.Hmed_R.ToString());
            
            propsPancakeCoil.ExpandAll();
        }
                
        private int convertStringToInt(string value)
        {
            int number;
            bool result = Int32.TryParse(value, out number);
            if (result) { return number; }
            else
            {
                //if (value == null) value = ""; 
                Console.WriteLine("Attempted conversion of '{0}' failed.", value);
                return 0;
            }

        }


    }
}
