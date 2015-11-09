#if ENABLE_UNET

namespace UnityEngine.Networking {
	[AddComponentMenu("Network/NetworkManagerHUD")]
	[RequireComponent(typeof(NetworkManager))]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class CustomNetworkManagerHUD : MonoBehaviour {
		public NetworkManager manager;
		[SerializeField]
		public bool showGUI = true;
		[SerializeField]
		public int offsetX;
		[SerializeField]
		public int offsetY;

		// Runtime variable
		bool showServer = false;

		void Awake() {
			manager = GetComponent<NetworkManager>();
		}

		void Update() {
			if (!showGUI)
				return;

			if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
				if (Input.GetKeyDown(KeyCode.S)) {
					manager.StartServer();
				}
				if (Input.GetKeyDown(KeyCode.H)) {
					manager.StartHost();
				}
				if (Input.GetKeyDown(KeyCode.C)) {
					manager.StartClient();
				}
			}
			if (NetworkServer.active && NetworkClient.active) {
				if (Input.GetKeyDown(KeyCode.K)) {
					manager.StopHost();
				}
			}
		}

		void OnGUI() {
			if (!showGUI)
				return;

			int xpos = 10 + offsetX;
			int ypos = 40 + offsetY;
			int spacing = 24;

			if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null) {
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Host(H)")) {
					manager.StartHost();
				}
				ypos += spacing;

				if (GUI.Button(new Rect(xpos, ypos, 105, 20), "LAN Client(C)")) {
					manager.StartClient();
				}
				manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), manager.networkAddress);
				ypos += spacing;

				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "LAN Server Only(S)")) {
					manager.StartServer();
				}
				ypos += spacing;
			}
			else {
				if (NetworkServer.active) {
					GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + manager.networkPort);
					ypos += spacing;
				}
				if (NetworkClient.active) {
					GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
					ypos += spacing;
				}
			}

			if (NetworkClient.active && !ClientScene.ready) {
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready")) {
					ClientScene.Ready(manager.client.connection);

					if (ClientScene.localPlayers.Count == 0) {
						ClientScene.AddPlayer(0);
					}
				}
				ypos += spacing;
			}

			if (NetworkServer.active || NetworkClient.active) {
				if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (K)")) {
					manager.StopHost();
				}
				ypos += spacing;
			}

			if (!NetworkServer.active && !NetworkClient.active) {
				ypos += 10;

				if (manager.matchMaker == null) {
					if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Enable Match Maker (M)")) {
						manager.StartMatchMaker();
					}
					ypos += spacing;
				}
				else {
					if (manager.matchInfo == null) {
						if (manager.matches == null) {
							if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Create Internet Match")) {
								manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
							}
							ypos += spacing;

							GUI.Label(new Rect(xpos, ypos, 100, 20), "Room Name:");
							manager.matchName = GUI.TextField(new Rect(xpos + 100, ypos, 100, 20), manager.matchName);
							ypos += spacing;

							ypos += 10;

							if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Find Internet Match")) {
								manager.matchMaker.ListMatches(0, 20, "", manager.OnMatchList);
							}
							ypos += spacing;
						}
						else {
							foreach (var match in manager.matches) {
								if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Join Match:" + match.name)) {
									manager.matchName = match.name;
									manager.matchSize = (uint) match.currentSize;
									manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
								}
								ypos += spacing;
							}
						}
					}

					if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Change MM server")) {
						showServer = !showServer;
					}
					if (showServer) {
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Local")) {
							manager.SetMatchHost("localhost", 1337, false);
							showServer = false;
						}
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Internet")) {
							manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
							showServer = false;
						}
						ypos += spacing;
						if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Staging")) {
							manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
							showServer = false;
						}
					}

					ypos += spacing;

					GUI.Label(new Rect(xpos, ypos, 300, 20), "MM Uri: " + manager.matchMaker.baseUri);
					ypos += spacing;

					if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Disable Match Maker")) {
						manager.StopMatchMaker();
					}
					ypos += spacing;
				}
			}
		}
	}
};
#endif //ENABLE_UNET
