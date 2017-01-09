using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class CubeController : MonoBehaviour {
    private const string LogDirectoryName = "logs";
    public bool OutputLogFile = true;
    public int LogOutputIntervalS = 1;
    private Vector3 mCurrentAngle = new Vector3(0, 0, 0);
    private Vector3 mCurrentScale = new Vector3(1, 1, 1);
    private Vector3 mCurrentPosition = new Vector3(0, 0, 0);
    private UH mUnlimitedHand;
    private UHController mUHController;
    private float mPrevUpdateTimeS = 0;
    private string mLogFileName;
    private FileStream mLogFileStream = null;
    private ArrayList mPrevAnglePR = null;
    private ArrayList mPrevAngleTempature = null;
    private ArrayList mPrevPhotoSensors = null;
    private ArrayList mPrevQuaternion = null;
    private ArrayList mPrevAccel = null;
    private ArrayList mPrevGyro = null;

    // Use this for initialization
    void Start () {
        // Create Log file name
        DateTime currentDateTime = DateTime.Now.ToLocalTime();
        mLogFileName = String.Format("{0}.txt", getCurrentDateTimeText());

        mUnlimitedHand = UH.global;
        mUHController = UHController.global;
        ChangeAngle(mCurrentAngle);
    }

    // Update is called once per frame
    void Update () {
        float currentTimeS = Time.time;
        if(currentTimeS > mPrevUpdateTimeS + LogOutputIntervalS)
        {
            mPrevUpdateTimeS = currentTimeS;

            if (mUnlimitedHand.Connected)
            {
                mUnlimitedHand.updateAnglePR(); // update UHPR and UHAngle
                mUnlimitedHand.updateAngleTempature();  // update UHAngle
                mUnlimitedHand.updatePhotoSensors();    // update UHPR
                mUnlimitedHand.updateQuaternion();  // update UHQuaternion
                mUnlimitedHand.updateQuaternionPR();    // update UHQuaternion and UHPR
                mUnlimitedHand.updateUH3DGyroAccel();   // update UHGyroAccelData

                ArrayList[] currentValues =
                {
                    new ArrayList((int[])mUnlimitedHand.UHPR.Clone()),
                    new ArrayList((int[])mUnlimitedHand.UHAngle.Clone()),
                    new ArrayList((int[])mUnlimitedHand.readPhotoSensors().Clone()),    // return UHPR
                    new ArrayList(mUnlimitedHand.UHQuaternion),
                    new ArrayList(mUnlimitedHand.readAccel()),  // return UHAccel(UHGyroAccelData[0] ~ UHGyroAccelData[2])
                    new ArrayList(mUnlimitedHand.readGyro()),   // return UHGyro(UHGyroAccelData[4] ~ UHGyroAccelData[6])
                };
                string[] currentValueNames =
                {
                    "AnglePR",
                    "AngleTempature",
                    "PhotoSensors",
                    "Quaternion",
                    "Accel",
                    "Gyro",
                };
                    string[] prevValueNames =
                    {
                    "mPrevAnglePR",
                    "mPrevAngleTempature",
                    "mPrevPhotoSensors",
                    "mPrevQuaternion",
                    "mPrevAccel",
                    "mPrevGyro",
                };
                Type thisClassType = this.GetType();

                for (int i = 0, sizeI = currentValues.Length; i < sizeI; i++)
                {
                    FieldInfo targetField = thisClassType.GetField(prevValueNames[i], BindingFlags.NonPublic | BindingFlags.Instance);
                    object fieldObject = targetField.GetValue(this);
                    StringBuilder logBuilder = new StringBuilder(currentValueNames[i]);
                    bool valueChanged = fieldObject != null ? !fieldObject.Equals(currentValues[i]) : true;
                    if (valueChanged)
                    {
                        ArrayList tempCurrentValueList = currentValues[i];
                        IEnumerator enumerator = currentValues[i].GetEnumerator();
                        for (int j = 0, sizeJ = currentValues[i].Count; j < sizeJ; j++)
                        {
                            if (enumerator.MoveNext())
                            {
                                logBuilder.Append("[").Append(j).Append("] ").Append(enumerator.Current).Append(", ");
                            }
                            else
                            {
                                break;
                            }
                        }
                        WriteLog(logBuilder.ToString());

                        targetField.SetValue(this, currentValues[i]);
                    }
                }
            }
            else
            {
                WriteLog("UnlimitedHand is not connected");
            }

            if (mUHController.IsFinishedCalibration())
            {
                
                int armStatus = mUHController.getArmStatus();
                ChangeAngleBy(new Vector3(0, 0, armStatus));
                //ChangePositionBy(new Vector3(armStatus / 1000, 0, 0));
            }
        }
    }

    public void OnDestroy()
    {

    }

    private string getCurrentDateTimeText()
    {
        DateTime currentDateTime = DateTime.Now.ToLocalTime();
        return String.Format("{0:0000}-{1:00}-{2:00}_{3:00}{4:00}{5:00}", currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);
    }

    private void prepareLogFile()
    {
        if (!Directory.Exists(LogDirectoryName))
        {
            Directory.CreateDirectory(LogDirectoryName);
        }
    }

    private void WriteLog(String content)
    {
        StringBuilder contentBuilder = new StringBuilder(getCurrentDateTimeText()).Append("\t").Append(content);

        if (OutputLogFile)
        {
            prepareLogFile();
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(LogDirectoryName + "\\" + mLogFileName, true, Encoding.UTF8);
                writer.WriteLine(contentBuilder.ToString());
            }
            finally
            {
                if(writer != null)
                {
                    writer.Close();
                }
            }
        }

        Debug.Log(contentBuilder.ToString());
    }

    public void ChangeAngle(Vector3 newAngle)
    {
        mCurrentAngle = newAngle;
        transform.Rotate(mCurrentAngle);
    }

    public void ChangeAngleBy(Vector3 diffAngle)
    {
        mCurrentAngle.Set(mCurrentAngle.x + diffAngle.x, mCurrentAngle.y + diffAngle.y, mCurrentAngle.z + diffAngle.z);
        transform.Rotate(mCurrentAngle);
    }

    public void ChangeScale(Vector3 newScale)
    {
        transform.localScale = mCurrentScale = newScale;
    }

    public void ChangeScaleBy(Vector3 diffScale)
    {
        mCurrentScale.Set(mCurrentScale.x + diffScale.x, mCurrentScale.y + diffScale.y, mCurrentScale.z + diffScale.z);
        transform.localScale = mCurrentScale;
    }

    public void ChangePosition(Vector3 newPosition)
    {
        transform.position = mCurrentPosition = newPosition;
    }

    public void ChangePositionBy(Vector3 diffPosition)
    {
        mCurrentPosition.Set(mCurrentPosition.x + diffPosition.x, mCurrentPosition.y + diffPosition.y, mCurrentPosition.z + diffPosition.z);
        transform.localScale = mCurrentPosition;
    }

    public void GetCurrentAngle(Vector3 angle)
    {
        if(angle != null)
        {
            angle.x = mCurrentAngle.x;
            angle.y = mCurrentAngle.y;
            angle.z = mCurrentAngle.z;
        }
    }

    public void GetCurrentScale(Vector3 scale)
    {
        if(scale != null)
        {
            scale.x = mCurrentScale.x;
            scale.y = mCurrentScale.y;
            scale.z = mCurrentScale.z;
        }
    }

    public void GetCurrentPosition(Vector3 position)
    {
        if (position != null)
        {
            position.x = mCurrentPosition.x;
            position.y = mCurrentPosition.y;
            position.z = mCurrentPosition.z;
        }
    }
}
