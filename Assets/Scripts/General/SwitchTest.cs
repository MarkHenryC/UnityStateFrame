using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public class SwitchTest
    {
        public string name;
        public SwitchTest(string pName) { name = pName; }
        public bool testedUpOpen, testedUpClosed, testedDownOpen, testedDownClosed, actionPending;        
        public void ResetTests() { testedUpOpen = testedUpClosed = testedDownOpen = testedDownClosed = actionPending = false; }
        public void TestStatus(bool isUp, bool isOpen)
        {
            if (isUp)
            {
                if (isOpen)
                {
                    testedUpOpen = true;

                    Debug.LogFormat("{0} up - open", name);
                }
                else
                {
                    testedUpClosed = true;

                    Debug.LogFormat("{0} up - closed", name);
                }
            }
            else
            {
                if (isOpen)
                {
                    testedDownOpen = true;

                    Debug.LogFormat("{0} down - open", name);
                }
                else
                {
                    testedDownClosed = true;

                    Debug.LogFormat("{0} down - closed", name);
                }
            }
        }

        /// <summary>
        /// This assumes a dual switch rig, so this switch needs 
        /// to be tested based on the mode of the other switch
        /// </summary>
        public bool TestComplete
        {
            get
            {
                return testedUpOpen && testedUpClosed && testedDownOpen && testedDownClosed;
            }
        }

        /// <summary>
        /// This assumes no other switches, so there's only 
        /// up/down which would be open/closed or closed/open
        /// </summary>
        public bool SingleSwitchTestComplete
        {
            get
            {
                return (testedUpOpen && testedDownClosed) ||
                    (testedUpClosed && testedDownOpen);
            }
        }
    }
}