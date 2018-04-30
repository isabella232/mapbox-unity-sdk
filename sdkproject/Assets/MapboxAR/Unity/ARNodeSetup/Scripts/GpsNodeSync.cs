﻿namespace Mapbox.Unity.Ar
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Utils;
	using Mapbox.Unity.Location;
	using System;
	using Mapbox.Unity.Map;
	using Mapbox.Utils;

	///<summary>
	///  Generates GPSNodes for CentralizedARLocator.
	/// </summary>
	public class GpsNodeSync : NodeSyncBase
	{
		[SerializeField]
		bool _filterNodes;

		[Tooltip("Applies only if FilterNode is true")]
		[SerializeField]
		float _desiredAccuracy = 5;

		[SerializeField]
		float _minMagnitude;

		AbstractMap _map;
		CircularBuffer<Node> _nodeBuffer;

		public override void InitializeNodeBase(AbstractMap map)
		{
			_map = map;
			IsNodeBaseInitialized = true;
			_nodeBuffer = new CircularBuffer<Node>(10);
			Debug.Log("Initialized GPS nodes");
		}

		private bool IsNodeGoodToUse(Location location)
		{
			// Check Node accuracy & distance.
			var latestNode = _map.GeoToWorldPosition(location.LatitudeLongitude);
			var previousNode = _map.GeoToWorldPosition(_nodeBuffer[0].LatLon);
			var forMagnitude = latestNode - previousNode;

			if (location.Accuracy <= _desiredAccuracy && _minMagnitude <= forMagnitude.magnitude)
			{
				// Node is good to use, return true
				return true;
			}
			else
			{
				//Bad node, discard. 
				return false;
			}
		}

		public override void SaveNode()
		{
			var location = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation;
			bool isFirstNode = (_nodeBuffer.Count == 0);
			bool isGoodFilteredNode = false;
			bool saveNode = true;
			if (isFirstNode)
			{
				saveNode = true;
			}
			else
			{
				isGoodFilteredNode = (_filterNodes && IsNodeGoodToUse(location));
				saveNode = true && ((!_filterNodes) || isGoodFilteredNode);
			}

			if (saveNode)
			{
				Debug.Log("Saving GPS Node");
				var latestNode = new Node
				{
					LatLon = location.LatitudeLongitude,
					Accuracy = location.Accuracy
				};

				_nodeBuffer.Add(latestNode);
			}
		}

		public override Node[] ReturnAllNodes()
		{
			var nodeArray = new Node[_nodeBuffer.Count];

			for (int i = 0; i < _nodeBuffer.Count; i++)
			{
				nodeArray[i] = _nodeBuffer[i];
			}

			return nodeArray;
		}

		public override Node ReturnNodeAtIndex(int index)
		{
			return _nodeBuffer[index];
		}

		public override int ReturnNodeCount()
		{
			return _nodeBuffer.Count;
		}

		public override Node ReturnLatestNode()
		{
			return _nodeBuffer[0];
		}
	}
}

