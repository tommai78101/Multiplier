using UnityEngine;
using System.Collections;

namespace Extension {
	public static class ExtensionClass {
		public static bool ReachedDestination(this NavMeshAgent agent) {
			if (!agent.pathPending) {
				if (agent.stoppingDistance > 0f) {
					if (agent.remainingDistance <= agent.stoppingDistance) {
						if (!agent.hasPath || agent.velocity.sqrMagnitude <= float.Epsilon) {
							return true;
						}
					}
				}
				else {
					if (!agent.hasPath || agent.velocity.sqrMagnitude <= float.Epsilon) {
						return true;
					}
				}
			}
			return false;
		}
	}
}
