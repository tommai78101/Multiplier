using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using MultiPlayer;
using Common;

public class InitializeUnitAttributes : NetworkBehaviour {
	public Attributes attributes;

	public void Start() {
		this.Invoke("FetchUnitAttributes", 0.5f);
	}

	public void FetchUnitAttributes() {
		GameObject[] objs = GameObject.FindGameObjectsWithTag("UnitAttributes");
		Debug.Log("There are " + objs.Length + " UnitAttribute objects.");
		for (int i = 0; i < objs.Length; i++) {
			UnitAttributes unitAttribute = objs[i].GetComponent<UnitAttributes>();
			if (unitAttribute != null) {
				NetworkIdentity identity = unitAttribute.GetComponent<NetworkIdentity>();
				if (identity != null && identity.hasAuthority) {
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.Health);
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.Attack);
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.AttackCooldown);
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.Speed);
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.Split);
					this.attributes.UpdateOnlineAttributes(unitAttribute, AttributeProperty.Merge);
				}
			}
		}
	}
}
