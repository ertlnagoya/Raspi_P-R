// using System.Drawing.Printing;
using System.Security.Cryptography.X509Certificates;
using RasPiMouse;
using UnityEngine;

namespace LineTrace.Handlers
{
    public enum CarAction
    {
        Go,
        Back,
        Cross
    }

    public class Handler
    {
        private Mouse mouse;
        private CarAction action;

        private GoHandler goHandler;
        private CrossHandler crossHandler;
        private BackHandler backHandler;

        private float t;
        private float maxT;

        private bool isCollision = false;

        private string RMColor;

        public Handler(Mouse mouse1, string color)
        {
            action = CarAction.Go;
            mouse = mouse1;
            goHandler = new GoHandler(mouse1);
            backHandler = new BackHandler(mouse1);
            crossHandler = new CrossHandler(mouse1);

            maxT = Random.Range(2, 3.5f);

            RMColor = color;
        }

        public void Handle()
        {
            if (action == CarAction.Back)
            {
                if (backHandler.Update()) action = CarAction.Go;
            } 
            else if (Mathf.Min(mouse.distSensor.Distance(0), mouse.distSensor.Distance(3)) < 0.1f)
            {
                t += Time.deltaTime;
                if (t > maxT)
                {
                    t = 0;
                    action = CarAction.Back;
                    backHandler.Reset();
                    if(!isCollision){
                        Debug.Log( "[Collision]" + " RM: " + RMColor + " time[s]: " +Time.time);
                        isCollision = true;
                    }
                }

                mouse.Stop();
            }
            else if (action == CarAction.Cross)
            {
                if (crossHandler.Update(mouse.transform)) action = CarAction.Go;
                isCollision = false;
            }
            else
            {
                t = 0;
                goHandler.Update();
                isCollision = false;
            }
        }

        public void SetCross(Vector3 center, Vector3 dest)
        {
            action = CarAction.Cross;
            crossHandler.Reset(center, dest);
        }

        public void SetBack()
        {
            action = CarAction.Back;
            backHandler.Reset();
        }
    }
}
