using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Notification
{
    public string Message;
    public float Duration;
    public bool Interrupt;
    public Color Color;

    public Notification(string message, float duration, bool interrupt, Color color)
    {
        this.Message = message;
        this.Duration = duration;
        this.Interrupt = interrupt;
        this.Color = color;
    }
}

public class NotificationManager : MonoBehaviour
{
    public Text notificationsText;
    public float duration;
    public Color color;

    public bool isNotifying = false;

    public bool interruptMessages;

    public static NotificationManager instance;

    public List<Notification> Notifications;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var newNotification = new Notification("Hello", 3, false, Color.blue);
            addNotification(newNotification);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            var newNotification = new Notification("Interrupting Message", 2, true, Color.red);
            addNotification(newNotification);
        }
        
    }

    public bool addNotification(Notification newNotification)
    {
        // if nofication is meant to interrupt, add it the top of the list and start notifying
        if (newNotification.Interrupt)
        {
            StopAllCoroutines();
            Notifications.Insert(0, newNotification);
            StartCoroutine(notify());
            return true;
        }
        
        // add regular notification to the end of the list
        Notifications.Add(newNotification);
        
        // if not already notifying, start notifying
        if (!isNotifying)
        {
            StartCoroutine(notify());
        }
        return Notifications.Count == 1;
    }

    private IEnumerator notify()
    {
        isNotifying = true;
        while(Notifications.Count > 0)
        {
            print(Notifications.Count);
            var currentNotification = Notifications[0];

            notificationsText.text = currentNotification.Message;
            notificationsText.color = currentNotification.Color;
            yield return new WaitForSeconds(currentNotification.Duration);
            // wait for completion of message to remove it
            Notifications.RemoveAt(0);
            yield return false;
            // sometimes depending on loop structure, it's catastrophic remove to items in containers at certain points in looped execution...
            // if (Notifications.Count <= 0)
            // {
            //     notificationsText.text = "";
            //     print("no more nots");
            //     break;
            // }

            // pause time?
            notificationsText.text = "";
            yield return new WaitForSeconds(1);
            yield return false;
        } 

        print("Notifications finished");
        isNotifying = false;
        yield return true;
    }
}
