using System.ComponentModel;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace PostNamazu.Models
{
	/// <summary>
	/// Waymark Model prepresents the data behind a Waymark.
	/// </summary>
	public class Waymark : INotifyPropertyChanged
	{
		/// <summary>
		/// X Coordinate of Waymark.
		/// </summary>
		public float X  {
            get => Marker.Position.X;
            set => Marker = Marker with {
                X = Actions.WayMark.IntEncode(value),
                Position = Marker.Position with {
                    X = value
                },
            };
        }


		/// <summary>
		/// Y Coordinate of Waymark.
		/// </summary>
		public float Y {
            get => Marker.Position.Y;
            set => Marker = Marker with {
                Y = Actions.WayMark.IntEncode(value),
                Position = Marker.Position with {
                    Y = value
                },
            };
        }

		/// <summary>
		/// Z Coordinate of Waymark.
		/// </summary>
		public float Z  {
            get => Marker.Position.Z;
            set => Marker = Marker with {
                Z = Actions.WayMark.IntEncode(value),
                Position = Marker.Position with {
                    Z = value
                },
            };
        }

		/// <summary>
		/// ID of Waymark.
		/// </summary>
		public WaymarkID ID { get; set; }

		/// <summary>
		/// Active state of the Waymark.
		/// </summary>
		public bool Active  {
            get => Marker.Active;
            set => Marker = Marker with {
                    Active = value
                };
        }
		public FieldMarker Marker { get; set; }

		public override int GetHashCode() => X.GetHashCode() & Y.GetHashCode() & Z.GetHashCode() & ID.GetHashCode() & Active.GetHashCode();

        public override string ToString() => Active 
			? $"{ID}: ({X:G3}, {Z:G3}), {Y:G3}" 
			: $"{ID}: inactive";

        public string ToJsonString() => Active
            ? $"\"X\": {X:G3}, \"Z\": {Z:G3}, \"Y\": {Y:G3}, \"Active\": true"
            : $"";

        /// <summary>
        /// PropertyChanged event handler for this model.
        /// </summary>
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
	}

	/// <summary>
	/// Waymark ID is the byte value of the waymark ID in memory.
	/// </summary>
	public enum WaymarkID : byte { A = 0, B, C, D, One, Two, Three, Four }
}
