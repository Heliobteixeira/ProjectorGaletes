using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Data;
using System.Data.SqlClient;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Gwen.Control;


namespace ProjectorGaletes
{
    struct ServerDefs
    {
        // Common definitions loaded on SQL Server
        public int maxCachedDays;
    }
    class Game : GameWindow
    {
        View view;
        private Vector2 viewInitPosition = new Vector2(0f,0f); // View center at X0 Y0
        private Vector2 objectOrigin = new Vector2(0f, 0f) ;   // Object origin at X0 Y0
        private Vector2 objectScale = new Vector2(1f, 1f);     // Object scale X1 Y1

        private PLCState plcState = new PLCState();  //Valores realtime obtidos do PLC

        private ModelGraphics WindingScene;

        private ServerDefs serverDefs;

        private float _drawRotation;

        public float drawRotation
        {
            get 
            {
                return -(plcState.angle + rotationOffset); 
            }
        }

        private float rotationOffset;

        private int _turnOffset;
        private int turnOffset
        {
            get { return _turnOffset; }
            set
            {
                if (coilIsSelected)
                {
                    PancakeCoil selectedCoil = conjuntoBobinagem[selectedCoilNbr];
                    if (value + plcState.turn >= 0 && value + plcState.turn <= selectedCoil.nrEsp) // 0<=Numero de espiras total<=NrEspirasGal !!!não previne se incrementarmos +-2...
                    {
                        _turnOffset = value;
                    }
                    WindingScene.currentTurn = turn;
                }
            }
        }

        private int turn
        {
            get { return plcState.turn + turnOffset; }
        }

        private Winding conjuntoBobinagem;

        private string _selectedProject;
        public string selectedProject
        {
            get
            {
                if (conjuntoBobinagem != null) { return conjuntoBobinagem.projecto; } else { return ""; }
            }
            set
            {
                _selectedProject = value;
            }
        }

        private int selectedCoilNbr;

        private bool coilIsSelected
        {

            get{if(conjuntoBobinagem != null && selectedCoilNbr > 0) {return true;} else {return false;}} 
        }

        private Gwen.Input.OpenTK input;
        private Gwen.Renderer.OpenTK renderer;
        private Gwen.Skin.Base skin;
        private Gwen.Control.Canvas canvas;
        private GUI gui;

        const int fps_frames = 50;
        private float keyPressMultiplier = 1.0f;
        private int globalBlendAlpha = 255; // Global Alpha variable for blinking/fading items
        private int globalBlendInterval = 500; // Milliseconds it takes to fade out/in
        private int globalAlphaInc = (byte)1;
        private readonly List<long> ftime;
        private readonly Stopwatch stopwatch;
        private long lastTime;
        private bool altDown = false;

        private bool guiNeedsUpdateAfterWindowResized = false;

        EventDrivenTCPClient client;

        const int TIMEOUTPLCPOOL = 250;
        private Timer tmrPLCPool = new Timer();
        
        string serialdatabuffer;

        public Game(int width, int height)
            : base(width, height)
        {
            GL.Enable(EnableCap.Texture2D);

            loadDefaultValueSettings();

            view = new View(viewInitPosition, 1.0, 0.0);
            objectScale = new Vector2(objectScale.X, objectScale.Y);

            //InputCust.Initialize(this);

            // Events necessários para o GUI:
            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyUp += Keyboard_KeyUp;

            
            MouseDown += Mouse_ButtonDown;
            MouseUp += Mouse_ButtonUp;
            MouseMove += Mouse_Move;
            MouseWheel += Mouse_Wheel;
            

            ftime = new List<long>(fps_frames);
            stopwatch = new Stopwatch();

            //Initialize the event driven client
            //client = new EventDrivenTCPClient(IPAddress.Parse("192.168.1.205"), 4001);
            client = new EventDrivenTCPClient(IPAddress.Parse("192.168.1.205"), 4001);
            //Initialize the events
            client.DataReceived += new EventDrivenTCPClient.delDataReceived(client_DataReceived);
            client.ConnectionStatusChanged += new EventDrivenTCPClient.delConnectionStatusChanged(client_ConnectionStatusChanged);

            tmrPLCPool.AutoReset = true;
            tmrPLCPool.Elapsed += new System.Timers.ElapsedEventHandler(tmrServerPLCTimeout_Elapsed);
            tmrPLCPool.Interval = TIMEOUTPLCPOOL;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            loadServerSettings();

            //texture = ContentPipe.LoadTexture("image.png");
            //tileset = ContentPipe.LoadTexture("TileSet1.png");
            //level = new Level(20, 20);

            GL.ClearColor(Color.White); //Background Color

            renderer = new Gwen.Renderer.OpenTK();
            skin = new Gwen.Skin.TexturedBase(renderer, "Content/DefaultSkin.png");
            //skin = new Gwen.Skin.Simple(renderer);
            //skin.DefaultFont = new Font(renderer, "Courier", 10);
            canvas = new Canvas(skin);

            input = new Gwen.Input.OpenTK(this);
            input.Initialize(canvas);

            canvas.SetSize(Width, Height);
            //canvas.ShouldDrawBackground = false;
            //canvas.BackgroundColor = Color.FromArgb(255, 150, 170, 170);

            //canvas.KeyboardInputEnabled = true;

            gui = new GUI(canvas, selectedNewCoilComboBox, selectedProjectChanged, scaleChanged, toggledPositioning);

            stopwatch.Restart();
            lastTime = 0;

            client.Connect();

            tmrPLCPool.Start();

            recalculateGlobalAlphaInc();

        }

        private void loadDefaultValueSettings()
        {

            viewInitPosition.X = ProjectorGaletes.Properties.Settings.Default.viewOrigin_X;
            viewInitPosition.Y = ProjectorGaletes.Properties.Settings.Default.viewOrigin_Y;

            objectOrigin.X = ProjectorGaletes.Properties.Settings.Default.objectOrigin_X;
            objectOrigin.Y = ProjectorGaletes.Properties.Settings.Default.objectOrigin_Y;

            objectScale.X = ProjectorGaletes.Properties.Settings.Default.objectScale_X;
            objectScale.Y = ProjectorGaletes.Properties.Settings.Default.objectScale_Y;

            rotationOffset = ProjectorGaletes.Properties.Settings.Default.rotationOffset;

            //turnOffset = ProjectorGaletes.Properties.Settings.Default.turnOffset;

            Console.WriteLine("Configuraçãoes carregadas...");
        }

        private void saveDefaultValueSettings()
        {
            ProjectorGaletes.Properties.Settings.Default.viewOrigin_X = (int)viewInitPosition.X;
            ProjectorGaletes.Properties.Settings.Default.viewOrigin_Y = (int)viewInitPosition.Y;

            ProjectorGaletes.Properties.Settings.Default.objectOrigin_X = (int)objectOrigin.X;
            ProjectorGaletes.Properties.Settings.Default.objectOrigin_Y = (int)objectOrigin.Y;

            ProjectorGaletes.Properties.Settings.Default.objectScale_X = objectScale.X;
            ProjectorGaletes.Properties.Settings.Default.objectScale_Y = objectScale.X;

            ProjectorGaletes.Properties.Settings.Default.rotationOffset = rotationOffset;

            //ProjectorGaletes.Properties.Settings.Default.turnOffset = turnOffset;

            ProjectorGaletes.Properties.Settings.Default.Save();

            // TODO: Save last selected winding/coil
            Console.WriteLine("Configuraçãoes guardadas...");
        }

        private void loadServerSettings()
        {
            string storedProcedure = GeneralConstants.storProcName_readServerDefs;
            SQLManager sqlManager = new SQLManager(ConfigurationManager.ConnectionStrings["DBConString"].ConnectionString);
            int maxCachedDays = -1;

            SqlParameter datePrmtr = SQLManager.newSqlParameter("@maxCachedDays", maxCachedDays, SqlDbType.Int, ParameterDirection.Output);

            SqlParameter[] parametrosSQL = {datePrmtr};

            sqlManager.executeStoredProcedure(storedProcedure, ref parametrosSQL);

            maxCachedDays = (int)datePrmtr.Value;
            serverDefs.maxCachedDays = maxCachedDays;

            sqlManager.Disconnect();
        }

        public override void Dispose()
        {
            canvas.Dispose();
            skin.Dispose();
            renderer.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The KeyboardDevice which generated this event.</param>
        /// <param name="e">The key that was pressed.</param>

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == global::OpenTK.Input.Key.Escape)
                Exit();
            else if (e.Key == global::OpenTK.Input.Key.AltLeft)
                altDown = true;
            else if (altDown && e.Key == global::OpenTK.Input.Key.Enter)
                if (WindowState == WindowState.Fullscreen)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Fullscreen;

            input.ProcessKeyDown(e);
        }

        void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            altDown = false;
            input.ProcessKeyUp(e);
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs args)
        {
            input.ProcessMouseMessage(args);
        }

        void Mouse_ButtonUp(object sender, MouseButtonEventArgs args)
        {
            input.ProcessMouseMessage(args); // CRASH
        }

        void Mouse_Move(object sender, MouseMoveEventArgs args)
        {
            input.ProcessMouseMessage(args);
        }

        void Mouse_Wheel(object sender, MouseWheelEventArgs args)
        {
            input.ProcessMouseMessage(args);
        }

 /*        
        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            Vector2 pos = new Vector2(e.Position.X, e.Position.Y);
            pos -= new Vector2(this.Width, this.Height) / 2f;
            pos = view.ToWorld(pos);

            view.SetPosition(pos, TweenType.QuadraticInOut, 60);
            
        
        }
*/

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            //view.rotation += 0.005f;
            //this.objectRotation += 0.05f;

            /*
            if (InputCust.MousePress(OpenTK.Input.MouseButton.Left))
            {
                Vector2 pos = new Vector2(Mouse.X, Mouse.Y) - new Vector2(this.Width, this.Height) / 2f;
                pos = view.ToWorld(pos);

                view.SetPosition(pos, TweenType.QuadraticInOut, 15);
                lastClickPos = pos;
            }

            if (InputCust.KeyDown(OpenTK.Input.Key.Right))
            {

                view.SetPosition(view.PositionGoTo + new Vector2(5, 0), TweenType.QuarticOut, 15);
            }

            if (InputCust.KeyDown(OpenTK.Input.Key.Left))
            {

                view.SetPosition(view.PositionGoTo + new Vector2(-5, 0), TweenType.QuarticOut, 15);
            }

            if (InputCust.KeyDown(OpenTK.Input.Key.Up))
            {

                view.SetPosition(view.PositionGoTo + new Vector2(0, -5), TweenType.QuarticOut, 15);
            }

            if (InputCust.KeyDown(OpenTK.Input.Key.Down))
            {

                view.SetPosition(view.PositionGoTo + new Vector2(0, 5), TweenType.QuarticOut, 15);
            }
            */ 

            view.Update();
            //InputCust.Update();

            updateStatistics();

        }

        private void updateStatistics()
        {
            if (ftime.Count == fps_frames)
                ftime.RemoveAt(0);

            ftime.Add(stopwatch.ElapsedMilliseconds - lastTime);
            lastTime = stopwatch.ElapsedMilliseconds;

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                gui.Note = String.Format("String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", renderer.TextCacheSize, renderer.DrawCallCount, renderer.VertexCount);
                gui.Fps = 1000f * ftime.Count / ftime.Sum();
                stopwatch.Restart();

                if (renderer.TextCacheSize > 1000) // each cached string is an allocated texture, flush the cache once in a while in your real project
                    renderer.FlushTextCache();
            }
        }

        private void recalculateGlobalAlphaInc()
        { 
            // Recalculates the Blending Alpha Increment so that it matches the desired globalBlendInterval
            globalAlphaInc = (int) (255 / (fps_frames * globalBlendInterval / 1000));
        }

        private void updateGlobalAlpha()
        {
            
            globalBlendAlpha = (globalBlendAlpha + globalAlphaInc);

            if (globalBlendAlpha >= 255)
            {
                globalBlendAlpha = 255;
                globalAlphaInc = -globalAlphaInc;
            }
            else if (globalBlendAlpha <= 0)
            {
                globalBlendAlpha = 0;
                globalAlphaInc = -globalAlphaInc;
            }

            WindingScene.blinkAlpha = globalBlendAlpha;

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.White);

            ModelGraphics.Begin(this.Width, this.Height); // ???
            view.ApplyTransform();


            if (coilIsSelected)
            {
                // Update GUI with current values before rendering 
                updateGUI();


                //Updates global alpha for fading effects
                updateGlobalAlpha();

                // Draw all the projection geometry
                DrawGeometry();

                setView();

                canvas.RenderCanvas(); // InvalidOperationException - object is currently in use elsewhere    

                if (guiNeedsUpdateAfterWindowResized)
                {
                    gui.repositionGridLabels();
                    guiNeedsUpdateAfterWindowResized = false;
                }
            }

            canvas.RenderCanvas(); // InvalidOperationException - object is currently in use elsewhere

            SwapBuffers();
        }

        private void selectedNewCoilComboBox(int newCoilNbr)
        {
            selectedCoilNbr = newCoilNbr;
            gui.coil = conjuntoBobinagem[selectedCoilNbr];
            turnOffset = 0; // Resets coil turn offset
            WindingScene.pancakeCoil = conjuntoBobinagem[selectedCoilNbr];
        }

        private void selectedProjectChanged(string newProject)
        {
            LoadWinding(newProject); //TODO: Handle if LoadWinding fails
        }

        private void cacheWindingDataToDB()
        {
            conjuntoBobinagem.cacheWindingToDB();
        }

        private bool getDBWindingData(string project)
        {
            return true;
        }

        private void scaleChanged(float scale)
        {
            this.objectScale.X = scale;
            this.objectScale.Y = scale;
        }

        //toggledPositioning
        private void toggledPositioning(bool toggledPositioningOn)
        {
            if (toggledPositioningOn)
            {
                this.KeyDown += KeyBoard_KeyDown;
            }
            else
            {
                this.KeyDown -= KeyBoard_KeyDown;
                this.saveDefaultValueSettings();
            }
            //this.scale.X = scale;
            //this.scale.Y = scale;
        }

        private void KeyBoard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                if (keyPressMultiplier < 20)
                {
                    keyPressMultiplier *= 1.05f;
                }
            }
            else
            {
                keyPressMultiplier = 1.0f;
            }
            switch (e.Key)
            {
                case Key.Left:
                    shiftOrigin(-1, 0);
                    break;
                case Key.Right:
                    shiftOrigin(1, 0);
                    break;
                case Key.Up:
                    shiftOrigin(0, -1);
                    break;
                case Key.Down:
                    shiftOrigin(0, 1);
                    break;
                case Key.Q:
                    increaseScale(0.001f);
                    break;
                case Key.A:
                    increaseScale(-0.001f);
                    break;
                case Key.W:
                    offsetRotation(0.1f);
                    break;
                case Key.S:
                    offsetRotation(-0.1f);
                    break;
                case Key.E:
                    turnOffset += 1;
                    break;
                case Key.D:
                    turnOffset -= 1;
                    break;
            }
            
        }

        private void shiftOrigin(int x, int y)
        {
            objectOrigin.X += (int)(x * keyPressMultiplier);
            objectOrigin.Y += (int)(y * keyPressMultiplier);
            //Updates origin on model
            WindingScene.modelOrigin = objectOrigin; 
        }

        private void increaseScale(float scaleFactor)
        {
            float auxScale = scaleFactor * keyPressMultiplier;
            objectScale.X += auxScale;
            objectScale.Y += auxScale;

            //Updates scale on model
            WindingScene.modelScale = objectScale;
        }

        private void offsetRotation(float rotationAngle)
        {
            rotationOffset += (float)(rotationAngle * keyPressMultiplier);

            //Updates scale on model
            WindingScene.modelRotation = rotationOffset;
        }

        private void updateGUI()
        {
            gui.decX = (int)objectOrigin.X;
            gui.decY = (int)objectOrigin.Y;
            gui.zoom = (float)(objectScale.X);
            gui.rotation = (float)rotationOffset;
            gui.offsetTurn = (int)turnOffset;

            gui.plcAngle = plcState.angle;
            gui.plcDirection = plcState.direction;
            gui.plcTurn = plcState.turn;
        }

        private void LoadWinding(string project)
        {
            int? daysSinceCache = Winding.getDaysSinceLastCached(project);

            if (daysSinceCache.HasValue) // ...has been cached
            {
                if (daysSinceCache < serverDefs.maxCachedDays)
                {
                    Console.WriteLine("Cached data is recent enough (< {0} days)", serverDefs.maxCachedDays);
                    conjuntoBobinagem = new Winding(project, cached: true);
                    Console.WriteLine("Loaded project {0}");

                }
                else
                {
                    Console.WriteLine("Cached data too old (Days since last cache:{0}; Max cached days: {1}). Loading Wintree data ", daysSinceCache, serverDefs.maxCachedDays);

                    conjuntoBobinagem = new Winding(project, cached: false); // cache = false -> forces fetching wintree data

                    Console.WriteLine("Loaded project {0}. Caching it's data to SQL DB...", project);
                    cacheWindingDataToDB();
                    Console.WriteLine("Caching sucesscefull");
                }
            }
            else
            {
                Console.WriteLine("{0} was never cached. Loading Wintree data", project);

                conjuntoBobinagem = new Winding(project, cached: false); // cache = false -> forces fetching wintree data

                Console.WriteLine("Loaded project {0}. Caching it's data to SQL DB...", project);
                cacheWindingDataToDB();
                Console.WriteLine("Caching sucesscefull");
            }
            

            if(conjuntoBobinagem.isLoaded) 
            {
                selectedProject = project;
                gui.fillUpAvailableCoilsComboBox(conjuntoBobinagem.coilsList());

                // Selecciona a menor galete da Winding
                selectedCoilNbr = conjuntoBobinagem.Keys.Min();

                //Actualiza a Scene
                WindingScene = new ModelGraphics(objectOrigin, Vector2.Zero, objectScale, 0, conjuntoBobinagem[selectedCoilNbr]);
            }

        }

        private void DrawGeometry()
        {
            WindingScene.Redraw();         
        }

        protected override void OnResize(EventArgs e)
        { //Ocorre imediatamente aquando inicio do resize o que não permite saber as dimensões da janela imediatamente            
            setView();
            guiNeedsUpdateAfterWindowResized = true;
            //canvas.RenderCanvas(); // InvalidOperationException - object is currently in use elsewhere
            //SwapBuffers();
            
        }

        private void setView()
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            canvas.SetSize(Width, Height);
        }

        // ----------- TCP Client Methods -----------
        //Fired when the connection status changes in the TCP client - parsing these messages is up to the developer
        //I'm just adding the .ToString() of the state enum to a richtextbox here

        void client_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            //Check if this event was fired on a different thread, if it is then we must invoke it on the UI thread
            /*
            if (InvokeRequired)
            {
                Invoke(new EventDrivenTCPClient.delConnectionStatusChanged(client_ConnectionStatusChanged), sender, status);
                return;
            }
             */
            Console.WriteLine("Connection: " + status.ToString());
        }
        //Fired when new data is received in the TCP client
        void client_DataReceived(EventDrivenTCPClient sender, object data)
        {
            //Again, check if this needs to be invoked in the UI thread
            /*
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new EventDrivenTCPClient.delDataReceived(client_DataReceived), sender, data);
                }
                catch
                { }
                return;
            }
            */

            //Interpret the received data object as a string
            string strData = data as string;
            /*
             * -O PLC deverá responder algo semelhante a :
             * @00RH00 0383 0000 06F6 06F6 52*CR
             * Onde:
             * 0383 : corresponde ao ângulo em hexadecimal (neste caso (0383)16=(899)10  que equivale a 89.9º)
             * 0000 : corresponde ao sentido em hexadecimal
             * 06F6 : corresponde à espira em hexadecimal (neste caso (06F6)16=(1782)10 )
            */

            /*string hexAngle = strData.Substring(7,4);
            string hexDirection = strData.Substring(11,4);
            string hexTurn = strData.Substring(15,4);
            string hexChecksum = strData.Substring(18,2);
            Console.WriteLine("HexAngle:" + hexAngle + " HexDirection:" + hexDirection + " HexTurn:" + hexTurn + " HexChecksum:" + hexChecksum);
            */
            //Console.WriteLine(strData);
            processDataReceived(strData);
        }

        void tmrServerPLCTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            poolPLC();
        }

        void poolPLC()
        {
            string data = "@00RH000200045C*" + Environment.NewLine;
            client.Send(data);
        }

        void processDataReceived(string data)
        {
            foreach (char character in data)
            {
                int charAsciiValue = (int)character;
                if (charAsciiValue==13)
                {
                    processDataReceivedComplete();
                }else{
                    serialdatabuffer += character;
                }
                
            }
        }

        void processDataReceivedComplete()
        { 
            //Console.WriteLine("Data Packet Complete: " + serialdatabuffer);
            //@00RH000383000006F606F652*CR
            if (parsePacketSucessfull())
            {
                serialdatabuffer = "";
            }
            else
            {
                if (serialdatabuffer.Length >= 26) // Packet is corrupt
                {
                    Console.WriteLine("Discarding corrupt packet: " + serialdatabuffer);
                }
                else
                {
                    Console.WriteLine("Packet Parse Unsucessfull");   
                }
                serialdatabuffer = "";
            }
        }

        bool parsePacketSucessfull()
        {
            if (serialdatabuffer.Length != 26)
            {
                Console.WriteLine("Invalid data packet: [" + serialdatabuffer + "] + Length: " + serialdatabuffer.Length);
                return false;
            }

            if (serialdatabuffer[0] != '@')
            {
                Console.WriteLine("Invalid data packet: [" + serialdatabuffer + "] + First Character: " + serialdatabuffer[0]);
                return false;
            }

            if (serialdatabuffer[serialdatabuffer.Length - 1] != '*')
            {
                Console.WriteLine("Invalid data packet: [" + serialdatabuffer + "] + Last Character: " + serialdatabuffer[serialdatabuffer.Length - 1]);
                return false;
            }

            string hexAngle = serialdatabuffer.Substring(7, 4);
            string hexDirection = serialdatabuffer.Substring(11, 4);
            string hexTurn = serialdatabuffer.Substring(15, 4);
            string hexChecksum = serialdatabuffer.Substring(18, 2);

            //Console.WriteLine("HexAngle:" + hexAngle + " HexDirection:" + hexDirection + " HexTurn:" + hexTurn + " HexChecksum:" + hexChecksum);


            int intAngle = Int32.Parse(hexAngle, System.Globalization.NumberStyles.HexNumber);
            int intDirection = Int32.Parse(hexDirection, System.Globalization.NumberStyles.HexNumber);
            int intTurn = Int32.Parse(hexTurn, System.Globalization.NumberStyles.HexNumber);
            int intChecksum = Int32.Parse(hexChecksum, System.Globalization.NumberStyles.HexNumber);

            Console.WriteLine("IntAngle:" + intAngle + " IntDirection:" + intDirection + " IntTurn:" + intTurn + " IntChecksum:" + intChecksum);

            //Se checksum OK então assume valores
            byte[] dataByteArray = Encoding.ASCII.GetBytes(serialdatabuffer.Substring(0, 23));

            string FCS = serialdatabuffer.Substring(23, 2); //WILL NOT FUNCTION WITH OTHER MESSAGES SINCE LENGTH IS NOT THE SAME

            if (!checkFCS(dataByteArray, FCS)) { return false; };

            // Set parsed PLC State:
            plcState.intangle = intAngle;
            plcState.direction = intDirection;
            plcState.turn = intTurn;

            //Console.WriteLine("Valid Packet Received ");

            return true;

        }

        bool checkFCS(byte[] byteArray, string FCS)
        {
            byte result = 0;
            foreach (byte b in byteArray)
            {
                result = (byte)(b ^ result);
            }

            string calculatedHexFCS = result.ToString("X");

            if (calculatedHexFCS == FCS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
