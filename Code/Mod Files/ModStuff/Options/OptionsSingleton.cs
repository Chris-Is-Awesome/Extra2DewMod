using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.Options
{
	/// <summary>
	/// Inherit from this base class to create a singleton.
	/// e.g. public class MyClassName : Singleton<MyClassName> {}
	/// </summary>
	public abstract class OptionsSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		// Functions to be inherited
		protected abstract void OnSceneLoad(Scene scene, bool isGameplayScene);
		protected abstract void OnFileLoad(bool isNew, string fileName = "", string filePath = "", RealDataSaver saver = null);
		protected abstract void OnPlayerSpawn(bool isRespawn);
		protected abstract void OnCollision(BC_Collider outgoingCollider, BC_Collider incomingCollider, BC_Collider.EventMode collisionType = BC_Collider.EventMode.Enter, bool isTrigger = false);
		protected abstract void OnRoomLoad(LevelRoom room);
		protected abstract void OnEnemySpawn(Entity ent);
		protected abstract void OnEnemyDeath(Entity deadEnt);

		public void SubscribeToEvents()
		{
			GameOptions.Instance.Startup();
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnFileLoad += OnFileLoad;
			GameStateNew.OnPlayerSpawn += OnPlayerSpawn;
			GameStateNew.OnCollision += OnCollision;
			GameStateNew.OnRoomChange += OnRoomLoad;
			GameStateNew.OnEntSpawn += OnEnemySpawn;
			GameStateNew.OnEnemyKill += OnEnemyDeath;
		}

		// Check to see if we're about to be destroyed.
		private static bool m_ShuttingDown = false;
		private static object m_Lock = new object();
		private static T m_Instance;

		/// <summary>
		/// Access singleton instance through this propriety.
		/// </summary>
		public static T Instance
		{
			get
			{
				if (m_ShuttingDown)
				{
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
						"' already destroyed. Returning null.");
					return null;
				}

				lock (m_Lock)
				{
					if (m_Instance == null)
					{
						// Search for existing instance.
						m_Instance = (T)FindObjectOfType(typeof(T));

						// Create new instance if one doesn't already exist.
						if (m_Instance == null)
						{
							// Need to create a new GameObject to attach the singleton to.
							var singletonObject = new GameObject();
							m_Instance = singletonObject.AddComponent<T>();
							//singletonObject.name = typeof(T).ToString() + " (Singleton)";

							// Make instance persistent.
							DontDestroyOnLoad(singletonObject);
						}
					}

					return m_Instance;
				}
			}
		}

		private void OnApplicationQuit()
		{
			m_ShuttingDown = true;
		}


		private void OnDestroy()
		{
			m_ShuttingDown = true;
		}
	}
}