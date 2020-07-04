using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class ConnectionReceiver : Receivable
	{
        public Connection connection;
        public string rolloverText;

        protected DynamicMaterial highlighter;

        protected override void Awake()
        {
            base.Awake();
            connection = GetComponentInParent<Connection>();

            Debug.Assert(connection, "No Connection in parent");

            highlighter = GetComponent<DynamicMaterial>();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            placeable = other.GetComponent<Placeable>() as Connectable;
            if (placeable)
            {
                placeable.NotifyEnterProximity(this);
                placeable.info = rolloverText;
            }
            if (highlighter)
                highlighter.Highlight(true);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (placeable)
            {
                placeable.NotifyExitProximity(this);
                placeable = null;
                if (visualCue)
                    visualCue.SetActive(false);
            }
            if (infoText)
                infoText.gameObject.SetActive(false);
            if (highlighter)
                highlighter.Highlight(false);
        }
    }
}