using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GCodeInterpreter
{
    //Enums
    public enum Comment { On, Off }
    public enum WorkingPlane { XY, XZ, YZ }
    public enum CoordinatesMode { Absolute, Relative }
    public enum PositionCoordinatesUnits { inch, mm }
    public enum Feed { On, Off }
    public enum FeedDirection { Pos, Neg }
    public enum FeedAcceleration { Begin } // assumed Acceleration at beginning always G08
    public enum FeedDeceleration { End, After }
    public enum Interpolation { Path, Time, None }
    public enum Motion { Rapid, Linear, ArcClkwise, ArcCtrClkwise, Dwell, None }

    class PrintState// : DependencyObject, INotifyPropertyChanged
    {
        //Fields
        private Comment comment_;
        private Motion motion_;
        private WorkingPlane working_plane_;
        private CoordinatesMode coordinates_mode_;
        private PositionCoordinatesUnits position_coordinates_units_;
        private Feed feed_;
        private FeedDirection feed_direction_;
        private FeedAcceleration feed_acceleration_;
        private FeedDeceleration feed_deceleration_;
        private Interpolation interpolation_;

        //Position needs to be modal
        //For example if Z coordinate is ommited, it keeps the current instead of 0.0
        private double[] position_;

        //Feedrate is modal
        //If next instruction does not contain any feedrate information, the previous is considered
        private double feed_rate_;

        //Notifies when property changes
        public delegate void PrinterStateChangedHandler(object sender, string name, int prop);
        public event PrinterStateChangedHandler PrinterStateChanged;

        //Default values constructor
        public PrintState()
        {
            comment = Comment.Off;
            motion = Motion.None;
            working_plane = WorkingPlane.XY;
            coordinates_mode = CoordinatesMode.Absolute;
            position_coordinates_units = PositionCoordinatesUnits.mm;
            feed = Feed.Off;
            feed_direction = FeedDirection.Pos;
            feed_acceleration = FeedAcceleration.Begin;
            feed_deceleration = FeedDeceleration.End;
            interpolation = Interpolation.None;

            position = new double[5];
            feed_rate = 0.0;
        }

        public PrintState ShallowCopy()
        {
            return (PrintState) this.MemberwiseClone();
        }

        public void CopyState(PrintState cp)
        {
            this.comment = cp.comment;
            this.motion = cp.motion;
            this.working_plane = cp.working_plane;
            this.coordinates_mode = cp.coordinates_mode;
            this.position_coordinates_units = cp.position_coordinates_units;
            this.feed = cp.feed;
            this.feed_direction = cp.feed_direction;
            this.feed_acceleration = cp.feed_acceleration;
            this.feed_deceleration = cp.feed_deceleration;
            this.interpolation = cp.interpolation;

            this.position = (double[])cp.position.Clone();
            this.feed_rate = cp.feed_rate;
        }

        public void ClearState()
        {
            comment = Comment.Off;
            motion = Motion.None;
            working_plane = WorkingPlane.XY;
            coordinates_mode = CoordinatesMode.Absolute;
            position_coordinates_units = PositionCoordinatesUnits.mm;
            feed = Feed.Off;
            feed_direction = FeedDirection.Pos;
            feed_acceleration = FeedAcceleration.Begin;
            feed_deceleration = FeedDeceleration.End;
            interpolation = Interpolation.None;

            position = new double[5];
            feed_rate = 0.0;
        }

        //Properties
        public Comment comment
        {
            get
            {
                return comment_;
            }

            set
            {
                comment_ = value;
                OnPrinterStateChanged("Comment", (int)value);
            }
        }

        public Motion motion
        {
            get
            {
                return motion_;
            }

            set
            {
                motion_ = value;
                OnPrinterStateChanged("Motion",(int)value);
            }
        }

        public WorkingPlane working_plane
        {
            get
            {
                return working_plane_;
            }

            set
            {
                working_plane_ = value;
                OnPrinterStateChanged("WorkingPlane", (int)value);
            }
        }

        public CoordinatesMode coordinates_mode
        {
            get
            {
                return coordinates_mode_;
            }

            set
            {
                coordinates_mode_ = value;
                OnPrinterStateChanged("CoordinatesMode", (int)value);
            }
        }

        public PositionCoordinatesUnits position_coordinates_units
        {
            get
            {
                return position_coordinates_units_;
            }

            set
            {
                position_coordinates_units_ = value;
                OnPrinterStateChanged("PositionCoordinatesUnits", (int)value);
            }
        }

        public Feed feed
        {
            get
            {
                return feed_;
            }

            set
            {
                feed_ = value;
                OnPrinterStateChanged("Feed", (int)value);
            }
        }

        public FeedDirection feed_direction
        {
            get
            {
                return feed_direction_;
            }

            set
            {
                feed_direction_ = value;
                OnPrinterStateChanged("FeedDirection", (int)value);
            }
        }

        public FeedAcceleration feed_acceleration
        {
            get
            {
                return feed_acceleration_;
            }

            set
            {
                feed_acceleration_ = value;
                OnPrinterStateChanged("FeedAcceleration", (int)value);
            }
        }

        public FeedDeceleration feed_deceleration
        {
            get
            {
                return feed_deceleration_;
            }

            set
            {
                feed_deceleration_ = value;
                OnPrinterStateChanged("FeedDeceleration", (int)value);
            }
        }

        public Interpolation interpolation
        {
            get
            {
                return interpolation_;
            }

            set
            {
                interpolation_ = value;
                OnPrinterStateChanged("Interpolation", (int)value);
            }
        }

        public double[] position
        {
            get
            {
                return position_;
            }

            set
            {
                position_ = value;
            }
        }

        public double feed_rate
        {
            get
            {
                return feed_rate_;
            }

            set
            {
                feed_rate_ = value;
            }
        }

        private void OnPrinterStateChanged(string name, int prop)
        {
            PrinterStateChangedHandler handler = PrinterStateChanged;
            if (handler != null)
            {
                handler(this, name, prop);
            }
        }

        public void PrinterState_PropertyChanged(object sender, string name, int prop)
        {
            switch(name)
            {
                case "Comment":
                    comment = (Comment)prop;
                    break;
                case "Motion":
                    motion = (Motion)prop;
                    break;
                case "WorkingPlane":
                    working_plane = (WorkingPlane)prop;
                    break;
                case "CoordinatesMode":
                    coordinates_mode = (CoordinatesMode)prop;
                    break;
                case "PositionCoordinatesUnits":
                    position_coordinates_units = (PositionCoordinatesUnits)prop;
                    break;
                case "Feed":
                    feed = (Feed)prop;
                    break;
                case "FeedDirection":
                    feed_direction = (FeedDirection)prop;
                    break;
                case "FeedAcceleration":
                    feed_acceleration = (FeedAcceleration)prop;
                    break;
                case "FeedDeceleration":
                    feed_deceleration = (FeedDeceleration)prop;
                    break;
                case "Interpolation":
                    interpolation = (Interpolation)prop;
                    break;
            }
        }

    }
}
