using System;
using OpenTK;
using System.Runtime.InteropServices;
using GameStack;

namespace GameStack.Graphics {
	public class Animation : IUpdater, IWaitFor {
		string _name;
		AnimationChannel[] _channels;
		double _duration, _rate;

		double _startTime;
		bool _isRunning;

		public Animation (string name, double duration, double rate, AnimationChannel[] channels) {
			_name = name;
			_duration = duration;
			_rate = rate == 0.0 ? 30.0 : rate;
			_channels = channels;
		}

		public string Name { get { return _name; } set { _name = value; } }

		public double Duration { get { return _duration; } }

		public double Rate { get { return _rate; } }

		public AnimationChannel[] Channels { get { return _channels; } }

		public void Start(double startTime) {
			_startTime = startTime;
			_isRunning = true;

			this.Apply(0.0);
		}

		public void Stop() {
			_isRunning = false;
		}

		public void Update (FrameArgs e) {
			if(_isRunning)
				this.Apply(e.Time - _startTime);
		}

		void Apply(double time) {
			time *= _rate;
			foreach (var chan in _channels)
				chan.Apply(time);
		}

		bool IWaitFor.Check () {
			return !_isRunning;
		}
	}

	public class AnimationChannel {
		string _nodeName;
		Node _node;
		AnimationBehavior _preState, _postState;
		PositionKey[] _posKeys;
		RotationKey[] _rotKeys;
		ScalingKey[] _scaleKeys;

		public AnimationChannel (string nodeName, AnimationBehavior preState, AnimationBehavior postState, PositionKey[] posKeys, RotationKey[] rotKeys, ScalingKey[] scaleKeys) {
			_nodeName = nodeName;
			_preState = preState;
			_postState = postState;
			_posKeys = posKeys;
			_rotKeys = rotKeys;
			_scaleKeys = scaleKeys;
		}

		public string NodeName { get { return _nodeName; } }

		public Node Node { get { return _node; } internal set { _node = value; } }

		public AnimationBehavior PreState { get { return _preState; } }

		public AnimationBehavior PostState { get { return _postState; } }

		public PositionKey[] PositionKeys { get { return _posKeys; } }

		public RotationKey[] RotationKeys { get { return _rotKeys; } }

		public ScalingKey[] ScalingKeys { get { return _scaleKeys; } }

		internal void Apply(double time) {
			// scaling
			Vector3 scale;
			if (_scaleKeys.Length < 2)
				scale = _scaleKeys[0].Scale;
			else {
				var keyTime = (time - _scaleKeys[0].Time) % (_scaleKeys[_scaleKeys.Length - 1].Time - _scaleKeys[0].Time);
				int i = 0;
				for (i = 0; i < _scaleKeys.Length; i++) {
					if (keyTime < _scaleKeys[i + 1].Time)
						break;
				}
				var prev = _scaleKeys[i];
				var next = _scaleKeys[i + 1];
				Vector3.Lerp(ref prev.Scale, ref next.Scale, (float)((keyTime - prev.Time) / (next.Time - prev.Time)), out scale);
			}
			var transform = Matrix4.Scale(scale);

			Quaternion rot;
			if (_rotKeys.Length < 2)
				rot = _rotKeys[0].Rotation;
			else {
				var keyTime = (time - _rotKeys[0].Time) % (_rotKeys[_rotKeys.Length - 1].Time - _rotKeys[0].Time);
				int i = 0;
				for (i = 0; i < _rotKeys.Length; i++) {
					if (keyTime < _rotKeys[i + 1].Time)
						break;
				}
				var prev = _rotKeys[i];
				var next = _rotKeys[i + 1];
				rot = Quaternion.Slerp(prev.Rotation, next.Rotation, (float)((keyTime - prev.Time) / (next.Time - prev.Time)));
			}
			var rotMat = Matrix4.Rotate(rot);
			Matrix4.Mult(ref transform, ref rotMat, out transform);

			// position
			Vector3 pos;
			if (_posKeys.Length < 2)
				pos = _posKeys[0].Position;
			else {
				var keyTime = (time - _posKeys[0].Time) % (_posKeys[_posKeys.Length - 1].Time - _posKeys[0].Time);
				int i = 0;
				for (i = 0; i < _posKeys.Length; i++) {
					if (keyTime < _posKeys[i + 1].Time)
						break;
				}
				var prev = _posKeys[i];
				var next = _posKeys[i + 1];
				Vector3.Lerp(ref prev.Position, ref next.Position, (float)((keyTime - prev.Time) / (next.Time - prev.Time)), out pos);
			}
			Matrix4 posMat;
			Matrix4.CreateTranslation(ref pos, out posMat);
			Matrix4.Mult(ref transform, ref posMat, out _node.transform);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PositionKey {
		public double Time;
		public Vector3 Position;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RotationKey {
		public double Time;
		public Quaternion Rotation;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ScalingKey {
		public double Time;
		public Vector3 Scale;
	}

	public enum AnimationBehavior {
		Default = 0x0,
		Constant = 0x1,
		Linear = 0x2,
		Repeat = 0x3
	}
}
