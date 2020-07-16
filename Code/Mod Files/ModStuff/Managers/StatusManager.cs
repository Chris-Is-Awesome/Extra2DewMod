using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class StatusManager : Singleton<StatusManager>
	{
		public List<StatusType> statuses = new List<StatusType>();
		Dictionary<StatusType, float> defaultTimers = new Dictionary<StatusType, float>();
		public bool hasSetStatus = false;

		// Gets all status effects
		public void GetStatuses()
		{
			foreach (StatusType status in Resources.FindObjectsOfTypeAll<StatusType>())
			{
				// Check if is applicable status (somehow the equipper is returned as a StatusType??)
				if (status.name.ToLower().Contains("stat_") && !statuses.Contains(status))
				{
					statuses.Add(status);
					defaultTimers.Add(status, status._data.GetComponent<TimedStatus>()._time);
				}
			}

			hasSetStatus = true;
		}

		// Activates the specified status by name
		public bool SetStatus(string statusName, bool isInfinite = false, float time = 0.0f, GameObject toEnt = null)
		{
			StatusType status = GetStatusType(statusName);
			EntityStatusable statusActivator;

			// Check if applying to ent
			if (toEnt == null)
			{
				// Apply to Ittle
				statusActivator = GameObject.Find("PlayerEnt").transform.Find("StatusAndEquips").GetComponent<EntityStatusable>();
			}
			else
			{
				// Apply to saved object
				statusActivator = toEnt.GetComponentInChildren<EntityStatusable>();
			}

			// Check if specified status is already active and if so, deactivate it
			if (!isInfinite && time <= 0)
			{
				for (int i = 0; i < statusActivator.currentStatuses.Count; i++)
				{
					StatusType statusToStop = statusActivator.currentStatuses[i].Type;

					if (statusActivator.currentStatuses[i].Type == status)
					{
						StopStatus(statusToStop, toEnt);
						return false;
					}
				}
			}

			TimedStatus timer = status._data.GetComponent<TimedStatus>();
			if (isInfinite) { time = 99999f; timer._time = time; }
			else if (time > 0f) { timer._time = time; }

			// Check if activator is null. This should never happen!
			if (statusActivator != null)
			{
				statusActivator.AddStatusType(status);
				statusActivator.Awake();

				// Save data to file so it saves on first use
				ModSaver.SaveFloatToFile("status/" + status.name, "time", time, true);
			}
			else { PlayerPrefs.SetString("test", "ERROR: [ModStuff.StatusManager.SetStatus()] says 'statusActivator is null'"); }

			hasSetStatus = true;
			return true;
		}

		public void StopStatus(StatusType status, GameObject toEnt = null)
		{
			EntityStatusable statusDeactivator;

			// Check if applying to ent
			if (toEnt == null)
			{
				// Apply to Ittle
				statusDeactivator = GameObject.Find("PlayerEnt").transform.Find("StatusAndEquips").GetComponent<EntityStatusable>();
			}
			else
			{
				// Apply to saved object
				statusDeactivator = toEnt.GetComponentInChildren<EntityStatusable>();
			}

			// Check if deactivator is null. This should never happen!
			if (statusDeactivator != null) { statusDeactivator.ClearStatus(status); }
			else { PlayerPrefs.SetString("test", "ERROR: [ModStuff.StatusManager.StopStatus()] says 'statusDeactivator is null'"); }

			hasSetStatus = false;
		}

		// Reset timers to default
		public void ResetTimers()
		{
			foreach (KeyValuePair<StatusType, float> keyValuePair in defaultTimers)
			{
				keyValuePair.Key._data.GetComponent<TimedStatus>()._time = keyValuePair.Value;
			}
		}

		// Returns the StatusType to activate
		public StatusType GetStatusType(string name)
		{
			StatusType status = null;

			for (int i = 0; i < statuses.Count; i++)
			{
				if (statuses[i].name.ToLower() == "stat_" + name.ToLower())
				{
					status = statuses[i];
					break;
				}
			}

			return status;
		}

		// Returns formatted name for status by index
		public string GetNameOfStatus(int indexForStatus)
		{
			if (statuses.Count > 0)
			{
				string statusName = statuses[indexForStatus].name;
				return (statusName.Substring(5));
			}

			return null;
		}

		// Returns formatted name for status by name
		public string GetNameOfStatus(string name)
		{
			if (statuses.Count > 0)
			{
				string statusName = "";

				for (int i = 0; i < statuses.Count; i++)
				{
					if (statuses[i].name.ToLower() == "stat_" + name.ToLower())
					{
						statusName = statuses[i].name;
						break;
					}
				}

				if (!string.IsNullOrEmpty(statusName))
				{
					return statusName.Substring(5);
				}
			}

			return null;
		}
	}
}