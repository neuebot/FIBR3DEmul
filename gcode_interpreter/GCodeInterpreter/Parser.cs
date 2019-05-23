using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Data;

/// <summary>
/// GCODE COMMANDS [G]
///     G00 - Rapid motion [m]
///     G01 - Linear motion [m]
///     G02 - Arc motion clkwise [m]
///     G03 - Arc motion counter clkwise [m]
///     G04 - Dwell
///     G08 - Acceleration at end of block (default) [m]
///     G09 - Deceleration at block end (depends G901 G900) [m]
///     G17 - Working plane XY
///     G18 - Working plane ZX
///     G19 - Working plane YZ
///!     G28 - Return to home
///!     G30 - Return to secondary home
///     G53 - Deselection of current offset
///     G54 - Selection of 1st offset
///     G55 - Selection of 2nd offset
///     G56 - Selection of 3rd offset
///     G57 - Selection of 4th offset
///     G58 - Selection of 5th offset
///     G59 - Selection of 6th offset
///     G60 - Stop (m)
///     G70 - Dimensions inch
///     G71 - Dimensions mm
///     G74 - Homing (sequential axis movement not supported, only parallel)
///     G90 - Absolute dimension
///     G91 - Incremental dimension
///!     G161 - Circle center point absolute [m] https://infosys.beckhoff.com/english.php?content=../content/1033/tf522x/2318588683.html&id=
///!     G162 - Circle center point relative (default) [m]
///     G193 - Path Interpolation - 1 command https://infosys.beckhoff.com/english.php?content=../content/1033/tccncprogramming/html/radiusprogrammingrg163.htm&id=
///     G293 - Time Interpolation - 1 command
///     G130 - Acceleration weight specific axis [m] https://infosys.beckhoff.com/english.php?content=../content/1033/tf522x/2318596363.html&id=
///     G131 - Acceleration weight all axes [m]
///     G231 - Acceleration weight all axes specific G00 [m]
///!     G132 - Weight of ramp time specific axis [m] https://infosys.beckhoff.com/english.php?content=../content/1033/tf522x/2318596363.html&id=
///!     G133 - Weight of ramp time all axes [m]
///!     G134 - Weight of geometric ramp time all axes [m]
///!     G233 - Weight of ramp time all axes specific G00 [m]
///         
///
/// GCODE COMMANDS [M]
///     M00 - Unconditional stop
///     M01 - Conditional stop
///     M02 - End of Program
///     M03 - Spindle clkwise
///     M04 - Spindle counter clkwise
///     M05 - Spindle stop
///     M06 - Tool change
///     M40 - Wait for temperature to settle
///     M41 - Change heated chamber temperature to: #val
///     M51 - Change bed temperature to: #val
///     M61 - Change extruder temperature to: #val
/// GCODE COORDINATES 
///     X - x-axis position
///     Y - y-axis position
///     Z - z-axis position
///     I - center x-axis position
///     J - center y-axis position
///     K - center z-axis position
///     A - filament advance
///     B - y-axis rotation
///     C - z-axis rotation
///     U,V,W - TODO Second movement coordinates
///     P,Q,R - TODO Third movement coordinates
///     
/// GCODE SPECIAL COMAMNDS
///     F - advance velocity
///     E - secondary velocity - velocity at end of block
///     D - TODO DUNNO
///     H - TODO DUNNO
///     L - TODO Repetition counter
///     S - TODO cut velocity (RPM)
///     T - TODO Tool number
/// </summary>


namespace GCodeInterpreter
{
    class Parser
    {
        //Acceleration ??? 2m/s^2
        public const double MAX_ACC = 2000.0;
        //Velocity mm/s
        public const double MAX_VEL = 50.0;
        public const double RAPID_VEL = 6000.0 / 60;

        private enum Coordinates { X,Y,Z,A,B,C,I,J,K,R,Num,F,E,S,_Last };

        //Parsing variables
        //List of raw g code as read from file
        List<string> FullGCode_;
        //Converted g code
        List<string> ParsedGCode_;
        PrintState ModalState;
        PrintState CurrentState; //non-modal
        //Report
        Dictionary<int, List<double>> StatisticsGCode_;
 
        //Interpret variables
        double max_acc;
        double[] m_accw;
        double[] m_accw_rapid;
        double[] m_cmd_vel;
        //double[] m_time_weights;
        //double[] m_time_weights_rapid;
        string m_command;
        string m_understand;
        int m_cur_line;

        bool m_desacelerate_next;
        double[] m_ref_offset;
        double[] m_home_offset_vel;

        public Parser()
        {
            //Decimal separator is the 'dot'
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = customCulture;

            //GCode Lists
            FullGCode_ = new List<string>();
            ParsedGCode_ = new List<string>();
            StatisticsGCode_ = new Dictionary<int, List<double>>();

            //Default printer states
            ModalState = new PrintState();
            CurrentState = new PrintState();
            //Unidirectional Binding of states: Modal -> Current
            ModalState.PrinterStateChanged += new PrintState.PrinterStateChangedHandler(CurrentState.PrinterState_PropertyChanged);

            //Default non-set parameters
            max_acc = MAX_ACC; //Maximum ramp acceleration
            m_accw = new double[] { 1.0, 1.0, 1.0 }; //Std acceleration weights
            m_accw_rapid = new double[] { 1.0, 1.0, 1.0 }; //Rapid acceleration weights
            //m_time_weights = new double[] { 1.0, 1.0, 1.0 };
            //m_time_weights_rapid = new double[] { 1.0, 1.0, 1.0 };

            //Only affect to the current command line
            //Velocity at index 1 is the current velocity
            //Velocities at indices 0 and 2 are different from index 1 only if there is a ramp
            //respectively before and after the current velocity.
            m_cmd_vel = new double[] { 0.0, 0.0, 0.0 };

            m_desacelerate_next = false;
            m_ref_offset = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 };
            double hovel = 500 / 60;
            m_home_offset_vel = new double[] { 0, hovel, 0 };

            //Parser pointer
            current_line = 0;
        }

        //will go private
        public void ParseBlock(int first_index, int last_index, bool always_print = false)
        {
            //Make sure to clear previous ParsedGCode
            ParsedGCode_.Clear();
            StatisticsGCode_.Clear();

            if (first_index > last_index)
                return;

            if (first_index < 0)
                first_index = 0;

            if (last_index > FullGCode_.Count)
                last_index = FullGCode_.Count;

            for (var i = first_index; i <= last_index; i++)
            {
                ParseSingleLine(i, always_print);
                if (!command.Equals("{}"))
                {
                    ParsedGCode_.Add(command);
                }
            }
        }

        // Function called to parse each line.  
        public void ParseSingleLine(int index, bool always_print = false)
        {
            current_line = index;
            command = "{}";
            //Regex to match expression
            Regex Gcode = new Regex("[;(=gmpxyzabcijkrfev][+-]?[0-9]*\\.?[0-9]*", RegexOptions.IgnoreCase);

            //Capture all elements of the line that match the regex command
            MatchCollection m = Gcode.Matches(FullGCode[index]);
            List<string> m_str = m.Cast<Match>().Select(n => n.Value).ToList();

            //Handle comments before parsing 
            //If the line contains "(" or ";", this element and all matches to the right of this element are ignored ().
            if (m_str.Contains("(") || m_str.Contains(";"))
            {
                int idx1 = m_str.FindIndex(x => x.StartsWith("("));
                int idx2 = m_str.FindIndex(x => x.StartsWith(";"));

                if (idx1 != -1)
                {
                    m_str.RemoveRange(idx1, m_str.Count - idx1);
                }

                if (idx2 != -1)
                {
                    m_str.RemoveRange(idx2, m_str.Count - idx2);
                }
            }

            /// <summary>
            /// In this code section we handle the List of strings separated by the Match command.
            /// 
            /// For the special case of characters like AV.A.ABS.A or AV.A.ACT_POS.A, the line is ignored too.
            /// 
            /// Any motion command contains coordinates. 
            /// 
            /// If the command starts with G0, G1... The nomenclature is normalized to G00, G01...
            /// If the modal state includes a motion, and the line only contains coordinates, the modal motion state is
            /// concatenated to the start of the line.
            /// 
            /// Then one of two methods follow: ParseMotion or ParseElse
            /// </summary>
            if (m_str.Count > 0)
            {
                //TODO: Handle these special cases
                if (m_str.Contains("V."))
                {
                    m_str.Clear();
                    understand = "Command Ignored";
                    return;
                }

                //Line with new motion - has coordinates
                List<string> read;
                int num;
                var coord = CommandCoord(m_str, out num, out read);
                if (read.Count > 0)
                {
                    //Interpret G0 as G00, G1 as G01, G2 as G02 and G3 as G03
                    if (m_str[0].Equals("G0"))
                        m_str[0] = "G00";
                    if (m_str[0].Equals("G1"))
                        m_str[0] = "G01";
                    if (m_str[0].Equals("G2"))
                        m_str[0] = "G02";
                    if (m_str[0].Equals("G3"))
                        m_str[0] = "G03";
                    if (m_str[0].Equals("G4"))
                        m_str[0] = "G04";

                    //Command does not start with motion word
                    // Either already in move
                    // Or no move
                    if (!m_str[0].Equals("G00") && !m_str[0].Equals("G01") && !m_str[0].Equals("G02") && 
                        !m_str[0].Equals("G03") && !m_str[0].Equals("G04") && !m_str[0].Equals("G74") &&
                        CurrentState.motion != Motion.None)
                    {
                        switch (CurrentState.motion)
                        {
                            case Motion.Rapid:
                                m_str.Insert(0, "G00");
                                break;
                            case Motion.Linear:
                                m_str.Insert(0, "G01");
                                break;
                            case Motion.ArcClkwise:
                                m_str.Insert(0, "G02");
                                break;
                            case Motion.ArcCtrClkwise:
                                m_str.Insert(0, "G03");
                                break;
                            case Motion.Dwell:
                                m_str.Insert(0, "G04");
                                break;
                        }
                        
                    }

                    //Handle Always Print
                    if (!m_str.Contains("A") && always_print)
                    {
                        m_str.Add("A1");
                    }

                    ParseMotion(m_str);
                }
                //Line with coords but no motion or with no coords
                ParseElse(m_str);
            }
            else
            {
                understand = "Comment or Empty line";
                return;
            }
        }

        // Peek Line - Displays the line command without affecting the parser states
        // Not the cleanest way, but the quickest way to handle this is to store a shallow 
        // copy of current states, parse line normally and then return the states back to 
        // their original values 
        public string PeekLine(int index)
        {
            PrintState ModalSafe = new PrintState();
            PrintState CurrentSafe = new PrintState();
            ModalSafe.CopyState(ModalState);
            CurrentSafe.CopyState(CurrentState);

            ParseSingleLine(index);

            string peek = understand;

            ModalState.CopyState(ModalSafe);
            CurrentState.CopyState(CurrentSafe);

            return peek;
        }

        //Reset Parser
        public void ResetParserStates()
        {
            ModalState.ClearState();
            CurrentState.ClearState();
        }

        private void ParseMotion(List<string> cmd)
        {
            //Velocities
            m_cmd_vel[2] = PeekNextVelocity();

            //Only G commands
            CommandG(cmd);

            m_cmd_vel[0] = m_cmd_vel[1];
            m_cmd_vel[1] = m_cmd_vel[2];

            //Copy ModalState to Current at new Motion
            CurrentState.CopyState(ModalState);
        }

        private void ParseElse(List<string> cmd)
        {
            if (cmd[0].StartsWith("G"))
            {
                CommandG(cmd);
            }
            else if (cmd[0].StartsWith("M"))
            {
                CommandM(cmd);
            }
            else
            {
                // if(m[0].Value.StartsWith("F") || m[0].Value.StartsWith("S") )
                var coord = CommandCoord(cmd);
            }
        }

        //Command Parsing
        private void CommandG(List<string> m)
        {
            Translator tr = new Translator();
            List<double> coord;
            int num;
            List<string> read;

            // TODO: sometimes there is G's after the first command
            // We assume all G's come in the beginning, and that the first dictates the motion,
            // the others are just options to the movement.

            // 1. check which other G's are in the line.
            // 2. run G's from second position onward
            // 3. then run normal switch
            foreach (string n in m)
            {
                switch (n)
                {
                    //Feed Adaptation
                    case "G08":
                        understand = "Feed acceleration at begin [modal]";
                        ModalState.feed_acceleration = FeedAcceleration.Begin;
                        break;
                    case "G09":
                        understand = "Feed deceleration at end [non-modal]";
                        CurrentState.feed_deceleration = FeedDeceleration.End;
                        break;
                    case "G900":
                        understand = "Feed deceleration at end [modal]";
                        ModalState.feed_deceleration = FeedDeceleration.End;
                        break;
                    case "G901":
                        understand = "Feed deceleration after end [modal]";
                        ModalState.feed_deceleration = FeedDeceleration.After;
                        break;

                    //Working Plane
                    case "G17":
                        understand = "Working plane XY [modal]";
                        ModalState.working_plane = WorkingPlane.XY;
                        break;
                    case "G18":
                        understand = "Working plane XZ [modal]";
                        ModalState.working_plane = WorkingPlane.XZ;
                        break;
                    case "G19":
                        understand = "Working plane YZ [modal]";
                        ModalState.working_plane = WorkingPlane.YZ;
                        break;

                    //TODO: ADD OFFSET FEATURE
                    case "G53":
                        understand = "Deselection of zero offset [modal]";
                        break;
                    case "G54":
                        understand = "Selection of 1st zero offset [modal]";
                        break;
                    case "G55":
                        understand = "Selection of 2nd zero offset [modal]";
                        break;
                    case "G56":
                        understand = "Selection of 3rd zero offset [modal]";
                        break;
                    case "G57":
                        understand = "Selection of 4th zero offset [modal]";
                        break;
                    case "G58":
                        understand = "Selection of 5th zero offset [modal]";
                        break;
                    case "G59":
                        understand = "Selection of 6th zero offset [modal]";
                        break;
                    case "G74":
                        {
                            //TODO: Implement individual axis Homing
                            coord = CommandCoord(m, out num, out read);
                            double[] move_idx = GetReferencePosition(coord, read, false);

                            double[] zero_pos = GetOffsetCoordinates();
                            //double[] zero_pos = {0.0, 0.0, 0.0, 0.0, 0.0};
                            ModalState.position = zero_pos;

                            double[] ref_acc = new double[] { max_acc * m_accw_rapid[0], max_acc * m_accw_rapid[1], max_acc * m_accw_rapid[2] };

                            //Get velocities - convert to mm/s from mm/min
                            double[] home_vel = m_home_offset_vel;

                            //Send Movement
                            command = tr.RapidMove(zero_pos, home_vel, ref_acc, false,
                                CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line + 1);
                        }
                        break;

                    case "G60": //accurate stop (position)
                        understand = "Accurate position stop [non-modal]";
                        break;

                    //Position Coordinate Units
                    case "G70":
                        understand = "Position coordinates in inches [modal]";
                        ModalState.position_coordinates_units = PositionCoordinatesUnits.inch;
                        break;
                    case "G71":
                        understand = "Position coordinates in mm [modal]";
                        ModalState.position_coordinates_units = PositionCoordinatesUnits.mm;
                        break;

                    //Coordinates Mode
                    case "G90":
                        understand = "Absolute coordinates [modal]";
                        ModalState.coordinates_mode = CoordinatesMode.Absolute;
                        break;
                    case "G91":
                    //TODO: relative coordinates 
                        understand = "Relative coordinates [modal]";
                        ModalState.coordinates_mode = CoordinatesMode.Relative;
                        break;
                    //TODO: set programmable offset
                    case "G92":
                        understand = "Set programmable offset [non-modal]";
                        coord = CommandCoord(m);
                        double[] ref_pos = new double[] { coord[(int)Coordinates.X], coord[(int)Coordinates.Y], coord[(int)Coordinates.Z] };
                        SetOffsetCoordinates(ref_pos);
                        // Parse coordinates next to the command
                        coord = CommandCoord(m, out num, out read);

                        if (ModalState.coordinates_mode == CoordinatesMode.Absolute)
                        {
                            coord = CommandCoord(m, out num, out read);
                            //Execute a G0 motion
                            //! ref_pos are the positions set as the offset
                            double[] ref_acc = new double[] { max_acc * m_accw_rapid[0], max_acc * m_accw_rapid[1], max_acc * m_accw_rapid[2] };
                            //Velocity Ramp
                            double[] ramp_vel;
                            if (read.Contains("F"))
                            {
                                m_cmd_vel[1] = coord[(int)Coordinates.F];
                                ModalState.feed_rate = m_cmd_vel[1];

                                //Get velocities - convert to mm/s from mm/min
                                ramp_vel = GetVelocityRamps();
                            }
                            else {
                                ramp_vel = m_home_offset_vel;
                            }

                            //Send Movement
                            command = tr.RapidMove(ref_pos, ramp_vel, ref_acc, CurrentState.feed == Feed.On,
                                CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line + 1);
                        }

                        break;
                    case "G193":
                        understand = "Path interpolation mode";
                        CurrentState.interpolation = Interpolation.Path;
                        break;
                    case "G293":
                        understand = "Time interpolation mode";
                        CurrentState.interpolation = Interpolation.Time;
                        break;

                    //Acceleration weights
                    case "G131":
                        {
                            understand = "Set group acceleration weights [modal]";
                            coord = CommandCoord(m, out num, out read);
                            int index = (int)Coordinates.Num;
                            m_accw = Array.ConvertAll(m_accw, x => coord[index]);
                            //If there are any specified coordinates after, they still change the acceleration weights
                            if (read.Contains("X"))
                            {
                                index = (int)Coordinates.X;
                                m_accw[0] = coord[index];
                            }
                            if (read.Contains("Y"))
                            {
                                index = (int)Coordinates.Y;
                                m_accw[1] = coord[index];
                            }
                            if (read.Contains("Z"))
                            {
                                index = (int)Coordinates.Z;
                                m_accw[2] = coord[index];
                            }
                        }
                        break;
                    case "G130":
                        {
                            understand = "Set specific acceleration weights [modal]";
                            int index;
                            coord = CommandCoord(m, out num, out read);
                            if (read.Contains("X"))
                            {
                                index = (int)Coordinates.X;
                                m_accw[0] = coord[index];
                            }
                            if (read.Contains("Y"))
                            {
                                index = (int)Coordinates.Y;
                                m_accw[1] = coord[index];
                            }
                            if (read.Contains("Z"))
                            {
                                index = (int)Coordinates.Z;
                                m_accw[2] = coord[index];
                            }
                        }
                        break;
                    case "G231":
                        {
                            understand = "Set group acceleration weights rapid [modal]";
                            coord = CommandCoord(m, out num, out read);
                            int index = (int)Coordinates.Num;
                            m_accw_rapid = Array.ConvertAll(m_accw, x => coord[index]);
                            //If there are any specified coordinates after, they still change the acceleration weights
                            if (read.Contains("X"))
                            {
                                index = (int)Coordinates.X;
                                m_accw_rapid[0] = coord[index];
                            }
                            if (read.Contains("Y"))
                            {
                                index = (int)Coordinates.Y;
                                m_accw_rapid[1] = coord[index];
                            }
                            if (read.Contains("Z"))
                            {
                                index = (int)Coordinates.Z;
                                m_accw_rapid[2] = coord[index];
                            }
                        }
                        break;
                }
            }

            // Movement switch
            switch (m[0])
            {
                // Movements              
                case "G00": //Rapid Move 
                    {
                        ModalState.motion = Motion.Rapid;
                        // Parse coordinates next to the command
                        coord = CommandCoord(m, out num, out read);
                        // Returned X, Y, Z, B, C
                        double[] ref_pos = GetReferencePosition(coord, read);
                        double[] ref_acc = new double[] { max_acc * m_accw_rapid[0], max_acc * m_accw_rapid[1], max_acc * m_accw_rapid[2] };
                        //Velocity Ramp - Always fastest velocity
                        if (read.Contains("F"))
                        {
                            m_cmd_vel[1] = coord[(int)Coordinates.F];
                            ModalState.feed_rate = m_cmd_vel[1];
                        }
                        if ( read.Contains("A"))
                        {
                            CurrentState.feed = Feed.On;
                        }
                        ////Get velocities - convert to mm/s from mm/min
                        //double[] ramp_vel = GetVelocityRamps();
                        double[] ramp_vel = GetVelocityRamps(RAPID_VEL);

                        //Send Movement
                        command = tr.RapidMove(ref_pos, ramp_vel, ref_acc, CurrentState.feed == Feed.On, 
                            CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line+1);
                    }
                    break;

                case "G01": //straight line
                    {
                        ModalState.motion = Motion.Linear;
                        // Parse coordinates next to the command
                        coord = CommandCoord(m, out num, out read);
                        // Returned X, Y, Z
                        double[] ref_pos = GetReferencePosition(coord, read);
                        double[] ref_acc = new double[] { max_acc * m_accw[0], max_acc * m_accw[1], max_acc * m_accw[2] };
                        //double[] ref_acc = new double[] { max_acc * m_accw_rapid[0], max_acc * m_accw_rapid[1], max_acc * m_accw_rapid[2] };
                        //Velocity Ramp
                        if (read.Contains("F"))
                        {
                            m_cmd_vel[1] = coord[(int)Coordinates.F];
                            ModalState.feed_rate = m_cmd_vel[1];
                        }
                        if (read.Contains("A"))
                        {
                            CurrentState.feed = Feed.On;
                        }
                        //Get velocities - convert to mm/s from mm/min
                        double[] ramp_vel = GetVelocityRamps();
                        //Send Movement
                        command = tr.LinearMove(ref_pos, ramp_vel, ref_acc, CurrentState.feed == Feed.On,
                            CurrentState.interpolation != Interpolation.None, CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line+1);
                    }
                    break;

                case "G02": //clockwise arc
                case "G03": //counterclockwise arc
                    {
                        bool clkwise = m[0].Equals("G02");
                        if(clkwise)
                        {
                            ModalState.motion = Motion.ArcClkwise;
                        }
                        else
                        {
                            ModalState.motion = Motion.ArcCtrClkwise;
                        }
                        // Parse coordinates next to the command
                        coord = CommandCoord(m, out num, out read);
                        double[] ref_acc = new double[] { max_acc * m_accw[0], max_acc * m_accw[1], max_acc * m_accw[2] };
                        //double[] ref_acc = new double[] { max_acc * m_accw_rapid[0], max_acc * m_accw_rapid[1], max_acc * m_accw_rapid[2] };
                        //Velocity Ramp
                        if (read.Contains("F"))
                        {
                            m_cmd_vel[1] = coord[(int)Coordinates.F];
                            ModalState.feed_rate = m_cmd_vel[1];
                        }
                        if (read.Contains("A"))
                        {
                            CurrentState.feed = Feed.On;
                        }
                        //Get velocities - convert to mm/s from mm/min
                        double[] ramp_vel = GetVelocityRamps();
                        // Contains XYR - semi circle with R
                        if (read.Contains("R") || read.Contains("U"))
                        {
                            double[] ref_pos = GetReferencePosition(coord, read);
                            double radius = coord[(int)Coordinates.R];
                            command = tr.ArcMove(ref_pos, radius, CurrentState.working_plane, clkwise, ramp_vel, ref_acc, CurrentState.feed == Feed.On,
                                CurrentState.interpolation != Interpolation.None, CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line+1);
                        }
                        //contains XYIJ - semi circle with ij
                        else if (read.Contains("X") || read.Contains("Y") || read.Contains("Z") || read.Contains("B") || read.Contains("C"))
                        {
                            double[] ref_pos = GetReferencePosition(coord, read);
                            //Center always relative to ref
                            double[] cen_pos = new double[] { coord[(int)Coordinates.I], coord[(int)Coordinates.J], coord[(int)Coordinates.K] };
                            command = tr.ArcMove(ref_pos, cen_pos, CurrentState.working_plane, clkwise, ramp_vel, ref_acc, CurrentState.feed == Feed.On,
                                CurrentState.interpolation != Interpolation.None, CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line+1);
                        }
                        //contains IJ - full circle
                        else if ((!read.Contains("X") && !read.Contains("Y") && !read.Contains("Z")) && (read.Contains("R") || (read.Contains("I") || read.Contains("J") || read.Contains("K"))))
                        {
                            double[] ref_pos = GetReferencePosition(coord, read);
                            double[] cen_pos = new double[] { coord[(int)Coordinates.I], coord[(int)Coordinates.J], coord[(int)Coordinates.K] };
                            command = tr.ArcMove(cen_pos, CurrentState.working_plane, clkwise, ramp_vel, ref_acc, CurrentState.feed == Feed.On,
                                CurrentState.interpolation != Interpolation.None, CurrentState.coordinates_mode == CoordinatesMode.Absolute, current_line+1);
                        }
                        else
                        {
                            // TODO: return error!
                        }
                    }
                    break;
                case "G04": //dwell program
                    {
                        CurrentState.motion = Motion.Dwell;
                        coord = CommandCoord(m, out num, out read);
                        // Either X01 or 01 (TODO) right now we assume the latter
                        double time = coord[7];
                        if (read.Contains("X"))
                        {
                            time = coord[0];
                        }
                        command = tr.Dwell(time, current_line+1);
                        break;
                    }
            }
        }

        private void CommandM(List<string> m)
        {
            // Only the first command can be M
            // Line that starts with M has no other commands
            switch (m[0])
            {
                //Change Feed
                case "M03":
                    understand = "Stop Feed";
                    //m_feed = false;
                    break;
                case "M04":
                    understand = "Start Feed";
                    //m_feed = true;
                    break;
                
                //End Program
                case "M02":
                case "M30":
                    understand = "Program ended.";
                    break;
                case "M40":
                    understand = "Wait for temperature to settle.";
                    break;
                case "M41":
                    understand = "Change heated chamber temperature to: " + m[1].Substring(1);
                    break;
                case "M51":
                    understand = "Change bed temperature to: " + m[1].Substring(1);
                    break;
                case "M61":
                    understand = "Change extruder temperature to: " + m[1].Substring(1);
                    break;

                default:
                    understand = "Command UNKNOWN.";
                    break;
            }
        }

        //Coordinates Parsing
        private List<double> CommandCoord(List<string> m)
        {
            List<string> temp;
            int num;
            return CommandCoord(m, out num, out temp);
        }

        private List<double> CommandCoord(List<string> m, out int num_coord, out List<string> cmd_read)
        {
            //List of command words read
            List<double> coord = new List<double>(new double[(int)Coordinates._Last]);
            //Returns the number of coordinates read
            //Coordinates are always associated to a new motion command
            //If this variable > 0, means that this is a motion command
            num_coord = 0; 
            //All command words read, be it coordinates or else
            cmd_read = new List<string>();

            foreach (string n in m)
            {
                double res;
                int index;
                string cmd = n.Substring(0, 1);
                //We want to store, what coord were received and which order
                cmd_read.Add(cmd);

                switch (cmd)
                {
                    case "X":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.X;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "Y":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.Y;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "Z":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.Z;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    //5-axis
                    case "B":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.B;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "C":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.C;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "I":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.I;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "J":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.J;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "K":
                        if(double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.K;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;
                    case "R": //both 'B' or 'U' can represent radius
                    case "U":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.R;
                            coord[index] = res;
                            num_coord++;
                        }
                        break;

                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        if (double.TryParse(n, out res))
                        {
                            index = (int)Coordinates.Num;
                            coord[index] = res;
                        }
                        break;

                    //Special characters - Not Coordinates (always calculated on peeknext)
                    case "F":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.F;
                            //Convert mm/min to mm/s
                            //coord[index] = res;
                            coord[index] = res / 60.0;
                        }
                        break;
                    case "A":
                        if (double.TryParse(n.Remove(0, 1), out res))
                        {
                            index = (int)Coordinates.A;
                            coord[index] = res;
                        }
                        break;
                    //case "P":
                    //    if(n.Substring(1,1).Equals("-"))
                    //    {
                    //        ModalState.feed_direction = FeedDirection.Neg;
                    //    }
                    //    else
                    //    {
                    //        ModalState.feed_direction = FeedDirection.Pos;
                    //    }
                    //    break;

                    default:
                        //Removes last element if it does not match the coordinates
                        cmd_read.RemoveAt(cmd_read.Count-1);
                        break;
                }
                
            }

            // Applying transformation to convention [inch] to [mm]
            if (CurrentState.position_coordinates_units == PositionCoordinatesUnits.inch)
            {
                coord = inch2mm(coord);
            }

            return coord;
        }

        private double [] GetOffsetCoordinates()
        {
            return m_ref_offset;
        }

        private void SetOffsetCoordinates(double[] offset)
        {
            m_ref_offset[0] = offset[0];
            m_ref_offset[1] = offset[1];
            m_ref_offset[2] = offset[2];

            if (offset.Length == 5)
            {
                m_ref_offset[3] = offset[3];
                m_ref_offset[4] = offset[4];
            }
        }

        //Class methods
        private double PeekNextVelocity()
        {
            double next_vel = CurrentState.feed_rate;
            //Iterate until finding the next move instruction
            int search_index = current_line;
            bool found = false;
            while (search_index < FullGCode.Count - 1 && !found)
            {
                search_index++;
                //Regex to match expression - 3-axis ("[%;(gmpxyzijkfe][+-]?[0-9]*\\.?[0-9]*"); 5-axis ("[%;(gmpxyzabcijkfe][+-]?[0-9]*\\.?[0-9]*")
                Regex Gcode = new Regex("[%;(gmpxyzabcijkrfe][+-]?[0-9]*\\.?[0-9]*", RegexOptions.IgnoreCase);

                MatchCollection m = Gcode.Matches(FullGCode[search_index]);
                int i = 0; //index
                if(m.Count == 0) //If no matches - skip
                {
                    continue;
                }
                if (m[0].Value.StartsWith("(") || m[0].Value.StartsWith(";")) //If line is a comment - skip
                {
                    continue;
                }
                if (m[0].Value.StartsWith("N")) //If line starts with N - ignore first word
                {
                    i = 1;
                }

                List<string> m_str = m.Cast<Match>().Select(n => n.Value).ToList();
                List<double> coord;
                int num_coord;
                List<string> read;
                coord = CommandCoord(m_str, out num_coord, out read);
                if (num_coord > 0) //Coordinates = motion
                {
                    if (read.Contains("F"))
                    {
                        next_vel = coord[(int)Coordinates.F];
                    }
                    found = true;
                }
                if (m[i].Equals("G04"))
                {
                    next_vel = 0.0; //stop
                    found = true;
                }
            }
            return next_vel;
        }

        private double[] GetVelocityRamps()
        {
            double[] ramp_vel = new double[3];
            m_cmd_vel.CopyTo(ramp_vel,0);
            if (CurrentState.feed_acceleration == FeedAcceleration.Begin && m_cmd_vel[2] > m_cmd_vel[1])
            {
                //does not accelerate on end of cmd
                ramp_vel[2] = ramp_vel[1]; 
            }
            if (CurrentState.feed_deceleration == FeedDeceleration.End && m_cmd_vel[0] > m_cmd_vel[1])
            {
                //does not decelerate at start
                if(!m_desacelerate_next)
                {
                    ramp_vel[0] = ramp_vel[1];
                    m_desacelerate_next = false;
                }
            }
            if (CurrentState.feed_deceleration == FeedDeceleration.After && m_cmd_vel[1] > m_cmd_vel[2])
            {
                //does not decelerate at end
                ramp_vel[2] = ramp_vel[1];
                m_desacelerate_next = true;
            }
            //Current velocity of next iteration will be the end velocity of this iteration
            m_cmd_vel[2] = ramp_vel[1];
            return ramp_vel;
        }

        //For G00 commands that always perform at MAX_VEL
        private double[] GetVelocityRamps(double command_velocity)
        {
            double[] ramp_vel = new double[3];
            m_cmd_vel.CopyTo(ramp_vel, 0);
            //if (CurrentState.feed_acceleration == FeedAcceleration.Begin && m_cmd_vel[2] > m_cmd_vel[1])
            //{
            //    //does not accelerate on end of cmd
            //    ramp_vel[2] = ramp_vel[1];
            //}
            //if (CurrentState.feed_deceleration == FeedDeceleration.End && m_cmd_vel[0] > m_cmd_vel[1])
            //{
            //    //does not decelerate at start
            //    if (!m_desacelerate_next)
            //    {
            //        ramp_vel[0] = ramp_vel[1];
            //        m_desacelerate_next = false;
            //    }
            //}
            //if (CurrentState.feed_deceleration == FeedDeceleration.After && m_cmd_vel[1] > m_cmd_vel[2])
            //{
            //    //does not decelerate at end
            //    ramp_vel[2] = ramp_vel[1];
            //    m_desacelerate_next = true;
            //}
            //Current velocity of next iteration will be the end velocity of this iteration
            //m_cmd_vel[2] = ramp_vel[1];
            ramp_vel[0] = command_velocity;
            ramp_vel[1] = command_velocity;
            ramp_vel[2] = command_velocity;
            return ramp_vel;
        }

        private double[] GetReferencePosition(List<double> coord, List<string> read)
        {
            double[] refpos = ModalState.position;
            //Velocity Ramp
            if (read.Contains("X"))
                refpos[0] = coord[(int)Coordinates.X] + m_ref_offset[0];
            if (read.Contains("Y"))
                refpos[1] = coord[(int)Coordinates.Y] + m_ref_offset[1];
            if (read.Contains("Z"))
                refpos[2] = coord[(int)Coordinates.Z] + m_ref_offset[2];
            if (read.Contains("B"))
                refpos[3] = coord[(int)Coordinates.B] + m_ref_offset[3];
            if (read.Contains("C"))
                refpos[4] = coord[(int)Coordinates.C] + m_ref_offset[4];

            //Store read position
            ModalState.position = refpos;

            double[] shallowcopy = (double[])refpos.Clone();
            return shallowcopy;
        }

        private double[] GetReferencePosition(List<double> coord, List<string> read, bool store)
        {
            double[] refpos = ModalState.position;
            //Velocity Ramp
            if (read.Contains("X"))
                refpos[0] = coord[(int)Coordinates.X] + m_ref_offset[0];
            if (read.Contains("Y"))
                refpos[1] = coord[(int)Coordinates.Y] + m_ref_offset[1];
            if (read.Contains("Z"))
                refpos[2] = coord[(int)Coordinates.Z] + m_ref_offset[2];
            if (read.Contains("B"))
                refpos[3] = coord[(int)Coordinates.B] + m_ref_offset[3];
            if (read.Contains("C"))
                refpos[4] = coord[(int)Coordinates.C] + m_ref_offset[4];

            if(store)
            {
                //Store read position
                ModalState.position = refpos;
            }
            double[] shallowcopy = (double[])refpos.Clone();
            return shallowcopy;
        }

        //Properties
        public List<string> FullGCode
        {
            get
            {
                return FullGCode_;
            }

            set
            {
                FullGCode_ = value;
            }
        }

        public List<string> ParsedGCode
        {
            get
            {
                return ParsedGCode_;
            }
        }

        public Dictionary<int, List<double>> StatisticsGCode
        {
            get
            {
                return StatisticsGCode_;
            }

            set
            {
                StatisticsGCode_ = value;
            }
        }

        public string command
        {
            get
            {
                return m_command;
            }

            private set
            {
                m_command = value;
                m_understand = value;
            }
        }

        public string understand
        {
            get
            {
                return m_understand;
            }

            private set
            {
                m_understand = value;
            }
        }

        public int current_line
        {
            get
            {
                return m_cur_line;
            }

            set
            {
                m_cur_line = value;
            }
        }

        //Utilities
        private double inch2mm(double inch)
        {
            return inch * 25.4;
        }

        private double[] inch2mm(double[] inch)
        {
            for (int i = 0; i < inch.Count(); ++i)
                inch[i] *= 25.4;
            return inch;
        }

        private List<double> inch2mm(List<double> inch)
        {
            for (int i = 0; i < inch.Count(); ++i)
                inch[i] *= 25.4;
            return inch;
        }
    }
}
