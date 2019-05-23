using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

//My includes
using System.IO;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using System.Drawing;

using System.Reflection;
using System.Text;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCodeInterpreter
{
    // Delegate type for hooking line change events.
    public delegate void ReadLineEventHandler(object sender, int line, bool collision);

    // Delegate type for hooking events of communication failure.
    // TODO: Consider adding info to type
    public delegate void CommunicationFailureEventHandler(object sender, string msg, string tip);

    // Delegate type for hooking events of collision line selected.
    public delegate void CollisionLineSelectedEventHandler(object sender, int line);

    public partial class InterpreterForm : Form
    {
        //GCode is loaded
        bool text_loaded = false;

        //Current parsing line
        string current_command;
        //Current read line information from the printer
        int read_line = 0;
        //Interface highlights the current read line
        bool highlight_line = false;
        //Interface highlights the current read line
        bool always_print = false;

        //Collection of collisions
        List<int> collision_lines = new List<int>();
        //Collision dialog
        Collisions collisionsBox;

        /// <summary>
        /// Background worker that will manage data communication of a large number of lines to parse.
        /// It may happen that the user requests the interpreter to read up to hundreds of thousands of lines.
        /// It is expected of the application manage the request, and continuously send data as the server (simulator) 
        /// reads the incoming lines.
        /// Parsing all these lines at once may cause the application to freeze, and inevitably will clog the communications
        /// and may lead to memory overflow.
        /// 
        /// This Background worker is responsible to keep track of the lines to parse and send, and the current line read.
        /// As each line is read, new information is sent to the parser and after that to the server.
        /// We keep the number of processed and communicated lines to a fixed amount to prevent the issues above.
        /// </summary>
        private System.ComponentModel.BackgroundWorker backgroundWorkerParser;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSender;

        private System.ComponentModel.BackgroundWorker backgroundWorkerReport;

        //Buffer send size - keeps track of current read line to avoid clogging communications
        //private const int queue_size = 200;
        //Concurrent queue of parser lines
        //private ConcurrentQueue<string> cq_parsing_lines;

        //Class that handles the gcode, interprets and sends motion specific commands to the actuators
        private Parser parser;

        //Class that handles external communication through sockets
        SocketClient socket_client = new SocketClient();

        public InterpreterForm()
        {
            InitializeComponent();
            InitializeBackgroundWorker();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            //My events
            socket_client.ReadLineEvent += new ReadLineEventHandler(HighlightLine);
            socket_client.CommunicationFailureEvent += new CommunicationFailureEventHandler(HandleCommunicationFailure);

            //GUI Variables
            highlight_line = highlightLineToolStripMenuItem.Checked;

            PropertyInfo aProp = typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(lGCodeView, true, null);
        }

        private void InterpreterForm_Closing()
        {
            socket_client.CloseClient();
        }

        // Set up the BackgroundWorker object by 
        // attaching event handlers. 
        private void InitializeBackgroundWorker()
        {
            backgroundWorkerParser = new BackgroundWorker();
            backgroundWorkerSender = new BackgroundWorker();
            backgroundWorkerReport = new BackgroundWorker();

            backgroundWorkerParser.WorkerReportsProgress = true;
            backgroundWorkerParser.WorkerSupportsCancellation = true;
            backgroundWorkerSender.WorkerReportsProgress = true;
            backgroundWorkerSender.WorkerSupportsCancellation = true;
            backgroundWorkerReport.WorkerReportsProgress = true;
            backgroundWorkerReport.WorkerSupportsCancellation = true;

            backgroundWorkerParser.DoWork += new DoWorkEventHandler(backgroundWorkerParser_DoWork);
            backgroundWorkerParser.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerParser_RunWorkerCompleted);

            backgroundWorkerSender.DoWork += new DoWorkEventHandler(backgroundWorkerSender_DoWork);

            backgroundWorkerReport.DoWork += new DoWorkEventHandler(backgroundWorkerReport_DoWork);
            backgroundWorkerReport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerReport_RunWorkerCompleted);
        }

        //File Menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dOpenFile.Title = "Select gcode file";
            //dOpenFile.InitialDirectory = @"C:\Users\Carlos\Desktop\FIBR3D\Working";
            dOpenFile.Filter = "GCode files|*.gcode|GCode CNC files|*.CNC|GCode NC files|*.nc|GCode Inventor Fusion|*.tap|Text file|*.txt|All files |*.*";
            dOpenFile.FilterIndex = 6;
            dOpenFile.RestoreDirectory = true;
            dOpenFile.ShowDialog();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(text_loaded)
            {
                parser.FullGCode.Clear();
                parser.ResetParserStates();
            }
            
            lGCodeView.BeginUpdate();
            lGCodeView.VirtualListSize = 10000000;
            lGCodeView.Items[0].Selected = true;
            lGCodeView.EnsureVisible(0);
            lGCodeView.EndUpdate();

            treeView1.Nodes.Clear();
            text_loaded = false;
            //lbCollisions.Items.Clear();

            toolStripLastRead.Text = "--";

            CommunicationEstablishEnables(false);
            ReadDocumentEnables(false);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> Parsed = new List<string>();

            //Runs task to retrieve the parsed list.
            BackgroundWorker bw = new BackgroundWorker();
            AutoResetEvent _resetEvent = new AutoResetEvent(false);
            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    Parsed = GetParsed();
                    _resetEvent.Set();
                });

            bw.RunWorkerAsync();

            dSaveFile.Filter = "G-code converted|*.gccf|Generic|*.txt";
            dSaveFile.Title = "Save an Interpret File";
            dSaveFile.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (dSaveFile.FileName != "")
            {
                TextWriter tw = new StreamWriter(dSaveFile.FileName);
                // Wait for signal of parsing completed
                _resetEvent.WaitOne();
                foreach (string s in Parsed)
                {
                    tw.WriteLine(s);
                }

                tw.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        //Edit Menu
        private void checkCollisionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(collisionsBox != null)
                collisionsBox.Close();

            collisionsBox = new Collisions(collision_lines);
            collisionsBox.CollisionLineSelectedEvent += new CollisionLineSelectedEventHandler(HandleCollisionLineSelected);
            collisionsBox.Show();
            collisionsBox.FormClosing += new FormClosingEventHandler(collisionsBox_FormClosing);
        }

        private void generateReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> Parsed = new List<string>();

            //Report box
            Report reportBox = new Report();

            //Runs task to retrieve the parsed list.
            BackgroundWorker bw = new BackgroundWorker();
            AutoResetEvent _resetEvent = new AutoResetEvent(false);
            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    Parsed = GetParsed();
                    _resetEvent.Set();
                });

            bw.RunWorkerAsync();

            //Wait for lines to be parsed
            _resetEvent.WaitOne();
            
            //Show Report box
            reportBox.Show();
        }

        private void generateReport(List<string> Parsed)
        {
            //Cancel Previous Operaions
            if (backgroundWorkerReport.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerReport.CancelAsync();
            }

            if (!backgroundWorkerReport.IsBusy)
            {
                // Start the asynchronous operation.
                backgroundWorkerReport.RunWorkerAsync(Parsed);
            }

        }

        //Handle collisionBox close event - unsubscribe EventHandlers
        private void collisionsBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            collisionsBox.CollisionLineSelectedEvent -= HandleCollisionLineSelected;
        }

        private void goToLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToLineForm gotolineBox = new GoToLineForm(lGCodeView.Items.Count);

            gotolineBox.ShowDialog();

            if (gotolineBox.success)
            {
                int line_num = gotolineBox.search_line - 1;
                lGCodeView.Items[line_num].Selected = true;
                lGCodeView.EnsureVisible(line_num);
            }
        }

        private void highlightLineToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            highlight_line = highlightLineToolStripMenuItem.Checked;
        }

        private void alwaysPrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            always_print = alwaysPrintToolStripMenuItem.Checked;
        }

        //Connection Menu
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (socket_client.StartClient() && text_loaded)
            {
                CommunicationEstablishEnables(true);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (socket_client.CloseClient())
            {
                read_line = 0;
                parser.ResetParserStates();

                toolStripLastRead.Text = "--";
                
                CommunicationEstablishEnables(false);
            }
        }

        //Send Menu
        private void nextLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int next_line = Math.Max(parser.current_line, read_line) + 1;

            if (next_line < parser.FullGCode.Count)
            {
                parser.current_line = next_line;
                parser.ParseSingleLine(next_line, always_print);
                current_command = parser.understand;
                GenerateInterpretText(parser.understand);

                //Avoid sending invalid messages that disrupt server
                if(SendMessageChecker(parser.command))
                {
                    socket_client.SendBlocking(parser.command);
                }
            }
        }

        private void singleLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            parser.ParseSingleLine(parser.current_line, always_print);
            current_command = parser.understand;
            GenerateInterpretText(current_command);

            //Avoid sending invalid messages that disrupt server
            if (SendMessageChecker(current_command))
            {
                socket_client.SendBlocking(current_command);
            }
            //if (SendMessageChecker(parser.command))
            //{
            //    socket_client.SendBlocking(parser.command);
            //}
        }

        private void fromLastLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //From last read command, until new clicked (current) line
            startAsync(read_line, parser.current_line);
        }

        private void allLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Assume that last sent line is the current line read
            // Start the asynchronous operation.
            startAsync(read_line, parser.FullGCode.Count - 1);
        }

        //About Menu
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        //Tool Strip
        private void toolStripOpen_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void toolStripClear_Click(object sender, EventArgs e)
        {
            clearToolStripMenuItem_Click(sender, e);
        }

        private void toolStripConnect_Click(object sender, EventArgs e)
        {
            connectToolStripMenuItem_Click(sender, e);
        }

        private void toolStripDisconnect_Click(object sender, EventArgs e)
        {
            disconnectToolStripMenuItem_Click(sender, e);
        }

        private void toolStripNextLine_Click(object sender, EventArgs e)
        {
            nextLineToolStripMenuItem_Click(sender, e);
        }

        private void toolStripSendLine_Click(object sender, EventArgs e)
        {
            singleLineToolStripMenuItem_Click(sender, e);
        }

        private void toolStripFromLast_Click(object sender, EventArgs e)
        {
            fromLastLineToolStripMenuItem_Click(sender, e);
        }

        private void toolStripSendAll_Click(object sender, EventArgs e)
        {
            allLinesToolStripMenuItem_Click(sender, e);
        }

        //Other
        private void dFile_FileOk(object sender, CancelEventArgs e)
        {
            //Class that handles the gcode, interprets and sends motion specific commands to the actuators
            parser = new Parser();

            //Load parser
            current_command = "Empty";

            //Store all lines of GCode in the global variable
            List<string> gcode = File.ReadAllLines(dOpenFile.FileName).ToList();
            parser.FullGCode = gcode;

            lGCodeView.BeginUpdate();
            lGCodeView.VirtualListSize = parser.FullGCode.Count;
            lGCodeView.EndUpdate();
            
            //Needs to be set true if a new file is loaded
            text_loaded = true;

            //Enable save button
            ReadDocumentEnables(true);

            if (socket_client.isConnected)
            {
                CommunicationEstablishEnables(true);
            }
        }

        //The basic VirtualMode function.  Dynamically returns a ListViewItem
        //with the required properties; in this case, the square of the index.
        private void lGCodeView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            //Caching is not required but improves performance on large sets.
            //To leave out caching, don't connect the CacheVirtualItems event 
            //and make sure myCache is null.
            ListViewItem lvi = new ListViewItem();  // create a listviewitem object
            NumberFormatInfo nfi = new CultureInfo("pt-PT").NumberFormat;
            nfi.NumberDecimalDigits = 0;
            lvi.Text = (e.ItemIndex + 1).ToString("n", nfi);

            ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
            if (text_loaded && parser.FullGCode.Count > 0 && e.ItemIndex >= 0 && e.ItemIndex < 0 + parser.FullGCode.Count)
            {
                lvsi.Text = parser.FullGCode[e.ItemIndex];        // assign the text to the item
                lvi.SubItems.Add(lvsi);                
            }
            else
            {
                lvsi.Text = "";        // assign the text to the item
                lvi.SubItems.Add(lvsi);
            }
            e.Item = lvi;
        }

        private void lGCodeView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (text_loaded && parser.FullGCode.Count > 0)
            {
                if ((lGCodeView.SelectedIndices != null) && (lGCodeView.SelectedIndices.Count > 0))
                {
                    int line_num = lGCodeView.SelectedIndices[0];
                    parser.current_line = line_num;
                    current_command = parser.PeekLine(line_num);
                    GenerateInterpretText(current_command);
                }                   
            }
        }

        private void lGCodeView_DragDrop(object sender, DragEventArgs e)
        {
            //Get file path
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);

            //Class that handles the gcode, interprets and sends motion specific commands to the actuators
            parser = new Parser();

            //Load parser
            current_command = "Empty";

            //Store all lines of GCode in the global variable
            List<string> gcode = File.ReadAllLines(file[0]).ToList();
            parser.FullGCode = gcode;

            lGCodeView.BeginUpdate();
            lGCodeView.VirtualListSize = parser.FullGCode.Count;
            lGCodeView.EndUpdate();

            //Needs to be set true if a new file is loaded
            text_loaded = true;

            //Enable save button
            ReadDocumentEnables(true);

            if (socket_client.isConnected)
            {
                CommunicationEstablishEnables(true);
            }
        }

        private void lGCodeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Get file path
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Check if Gcode
                if (String.Equals(Path.GetExtension(file[0]), ".gcode", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(Path.GetExtension(file[0]), ".cnc", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(Path.GetExtension(file[0]), ".nc", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(Path.GetExtension(file[0]), ".tap", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(Path.GetExtension(file[0]), ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void HighlightLine(object sender, int line, bool collision)
        {
            //This catches the event call and invokes it instead, from the current thread
            //Necessary because, WinForm components cannot be accessed outside the UI thread
            if (InvokeRequired)
            {
                ReadLineEventHandler cb = new ReadLineEventHandler(HighlightLine);
                Invoke(cb, new object[] { sender, line, collision });
                return;
            }

            //Get index char from the first index of specified line
            //Textbox uses a continuous index, because new line is just another char.
            read_line = line - 1;
            toolStripLastRead.Text = line.ToString();

            //Highlights Line in Box - OFF 
            if (highlight_line)
            {
                lGCodeView.Items[read_line].Selected = true;
                lGCodeView.EnsureVisible(read_line);
            }

            //Add value to collisions list
            if(collision)
            {
                //Variable to store the collision lines
                collision_lines.Add(read_line);
                collisionLabel.Text = "Collisions " + collision_lines.Count;

                if(collisionsBox != null)
                {
                    collisionsBox.AddCollision(read_line);
                }
            }
        }

        //Parse all GCode
        private List<string> GetParsed()
        {
            if (text_loaded && parser.FullGCode.Count > 0)
            {
                //From last read command, until new clicked (current) line
                parser.ParseBlock(read_line, parser.FullGCode.Count-1, always_print);

                return parser.ParsedGCode;
            }
            return null;
        }

        private void ReadDocumentEnables(bool enable)
        {
            //Enable connection and save buttons
            connectToolStripMenuItem.Enabled = enable;
            toolStripConnect.Enabled = enable;
            disconnectToolStripMenuItem.Enabled = enable;
            toolStripDisconnect.Enabled = enable;
            goToLineToolStripMenuItem.Enabled = enable;
            saveToolStripMenuItem.Enabled = enable;
        }

        private void CommunicationEstablishEnables(bool enable)
        {
            toolStripNextLine.Enabled = enable;
            toolStripSendLine.Enabled = enable;
            toolStripFromLast.Enabled = enable;
            toolStripSendAll.Enabled = enable;

            nextLineToolStripMenuItem.Enabled = enable;
            singleLineToolStripMenuItem.Enabled = enable;
            fromLastLineToolStripMenuItem.Enabled = enable;
            allLinesToolStripMenuItem.Enabled = enable;
        }

        //BackgroundWorker Callbacks
        private void startAsync(int start_line, int last_line)
        {
            cancelAsync();

            Tuple<int, int> to_send_lines = new Tuple<int, int>(start_line, last_line);
            
            if (backgroundWorkerParser.IsBusy != true && backgroundWorkerSender.IsBusy != true)
            {
                //parsing_complete = false;
                // Start the asynchronous operation.
                backgroundWorkerParser.RunWorkerAsync(to_send_lines);
                //backgroundWorkerSender.RunWorkerAsync(to_send_lines);
            }
        }

        private void cancelAsync()
        {
            if (backgroundWorkerParser.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerParser.CancelAsync();
            }

            if (backgroundWorkerSender.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerSender.CancelAsync();
            }
        }

        // This event handler is where the time-consuming work is done.
        private void backgroundWorkerParser_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //Starting and Ending lines
            Tuple<int, int> args = (Tuple<int, int>)e.Argument;
            int start_line = args.Item1;
            int end_line = args.Item2;

            ParseDataBlock(args, worker, e);
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerParser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //parsing_complete = true;
            backgroundWorkerSender.RunWorkerAsync();
        }

        // This event handler is where the time-consuming work is done.
        private void backgroundWorkerSender_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            SendDataBlock(worker, e);
        }

        // This event handler is where the time-consuming work is done.
        private void backgroundWorkerReport_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //Starting and Ending lines
            List<string> args = (List<string>)e.Argument;


            //ParseDataBlock(args, worker, e);
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //parsing_complete = true;
            backgroundWorkerReport.RunWorkerAsync();
        }

        private void ParseDataBlock(Tuple<int, int> block_lines, BackgroundWorker worker, DoWorkEventArgs e)
        {
            parser.ParseBlock(block_lines.Item1, block_lines.Item2, always_print);
        }

        private bool SendDataBlock(BackgroundWorker worker, DoWorkEventArgs e)
        {
            List<string> parsed = parser.ParsedGCode;

            for(int i = 0; i < parsed.Count; i++)
            {
                //Avoid sending invalid messages that disrupt server
                if (SendMessageChecker(parsed[i]))
                {
                    if (socket_client.SendBlocking(parsed[i]))
                    {
                        //Requires all threads to run!
                        //tbInterpret.Text = "Sent line: " + start_send_line + i;
                    }
                    else
                    {
                        //Something
                        return false;
                    }
                }
            }

            //Everything went ok
            return true;
        }

        private void GenerateInterpretText(string understand)
        {
            understand = understand.Trim();
            if ((understand.StartsWith("{") && understand.EndsWith("}")) || //For object
                (understand.StartsWith("[") && understand.EndsWith("]"))) //For array
            {
                try
                {
                    LoadJsonToTreeView(treeView1, understand);
                    treeView1.Nodes[0].Expand();
                    treeView1.Nodes[0].Nodes[6].Expand();
                    treeView1.Nodes[0].Nodes[9].Expand();
                    treeView1.Nodes[0].Nodes[10].Expand();
                    //Info text
                    infoLabel.Text = "Command" + treeView1.Nodes[0].Nodes[4].Text.Substring(4);
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                if(treeView1.Nodes.Count > 0)
                {
                    treeView1.Nodes.Clear();
                }
                infoLabel.Text = understand;
            }
        }

        void LoadJsonToTreeView(TreeView treeView, string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var @object = JObject.Parse(json);
            AddObjectNodes(@object, "GCodeJSON", treeView.Nodes);
        }

        void AddObjectNodes(JObject @object, string name, TreeNodeCollection parent)
        {
            var node = new TreeNode(name);
            parent.Clear();
            parent.Add(node);

            foreach (var property in @object.Properties())
            {
                AddTokenNodes(property.Value, property.Name, node.Nodes);
            }
        }

        void AddArrayNodes(JArray array, string name, TreeNodeCollection parent)
        {
            var node = new TreeNode(name);
            parent.Add(node);

            for (var i = 0; i < array.Count; i++)
            {
                AddTokenNodes(array[i], string.Format("[{0}]", i), node.Nodes);
            }
        }

        void AddTokenNodes(JToken token, string name, TreeNodeCollection parent)
        {
            if (token is JValue)
            {
                var value = ((JValue)token).Value;
                if (value is double)
                {
                    value = Math.Truncate((double)value * 1000) / 1000;
                }
                parent.Add(new TreeNode(string.Format("{0}: {1}", name, value)));
            }
            else if (token is JArray)
            {
                AddArrayNodes((JArray)token, name, parent);
            }
            else if (token is JObject)
            {
                AddObjectNodes((JObject)token, name, parent);
            }
        }

        private void HandleCommunicationFailure(object sender, string msg, string tip)
        {
            string emsg = string.Format("{0}\n\n{1}", msg, tip);
            MessageBox.Show(emsg,
                "Communication Failure",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);

            EventArgs e = new EventArgs();
            //Checks whether the communication thread is making a call to the form thread
            if (toolStrip1.InvokeRequired)
            {
                ///If so, it executes the specified delegate on the thread that owns the control's underlying
                ///window handle. 
                BeginInvoke((MethodInvoker)delegate
                {
                    disconnectToolStripMenuItem_Click(sender, e);
                });
            }
            else
            {
                disconnectToolStripMenuItem_Click(sender, e);
            }
        }

        private void HandleCollisionLineSelected(object sender, int line)
        {
            // If the item is valid
            if (line != -1)
            {
                int index = collision_lines[line];

                lGCodeView.Focus();
                lGCodeView.Items[index].Selected = true;
                lGCodeView.EnsureVisible(index);
            }
        }

        private void collisionLabel_Click(object sender, EventArgs e)
        {
            checkCollisionsToolStripMenuItem_Click(sender, e);
        }

        private bool SendMessageChecker(string msg)
        {
            return !(msg == "{}");
        }
    }
}
