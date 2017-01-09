using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.IO.Ports;
using System.Threading;


public class UHController : MonoBehaviour
{
    private const int ArmStatusBorder = 12;
    private const int PhotoSensorCount = 8;

    private UH mUH;

    //UH instance
    private static UHController sInstance = null;
    public static UHController global { get { return sInstance; } }

    /**** Object: Forearm Angle    *************************************************/
    public int[] handAngle = new int[3];

    /*** Object: Sensors' Values for the Hand Movements(Photo-Reflectors' Values)***/
    public int[] uPR = new int[PhotoSensorCount];      //Current Sensors' Values    

    [HideInInspector]
    public int[] uPRSum = new int[PhotoSensorCount];   //Sum of the Sensors' Values 

    [HideInInspector]
    public int count;                   //Count Number to calculate the Averages of the Sensor's Values

    [HideInInspector]
    public int[] uPRAve = new int[PhotoSensorCount];   //Averages of the Sensor's Values

    [HideInInspector]
    public int[] mAngleFlat = new int[10];//Past Values of an angle

    [HideInInspector]
    public int mAngleFlatAve;            //Average of "angleFlat"

    [HideInInspector]
    public int mCalibCount;              //The Count of the calibration

    [HideInInspector]
    public bool mPRCalib;

    public int mPRVARSum;                //Finger Movement SUM

    private int mEachFingerPRVARSum;    //Each Finger Movement SUM

    [HideInInspector]
    public int PRVARSumAVE;

    [HideInInspector]
    public int PRVARSumCount;

    public int FIRE_THRESHOLD = 120;    //Fire Threshold which compares with "mPRVARSum"

    /*** Object: Feedback Flags *********************************************************/
    private bool isStimulating;			//Flag of the feedback using very week EMS(Electric Muscle Stimulation)

    /////////////////////////////////////////////////////
    /// AWAKE
    /////////////////////////////////////////////////////
    void Awake()
    {
        Debug.Log("UnlimitedHand awake");
        if (sInstance == null) sInstance = this;
        DontDestroyOnLoad(this);
    }

    /////////////////////////////////////////////////////
    /// START
    /////////////////////////////////////////////////////
    private void Start()
    {
        /****  Additional Code for UnlimitedHand  ********************************/
        mPRCalib = true;
        isStimulating = false;

        count = 1;
        mCalibCount = 0;
        mPRVARSum = 0;
        PRVARSumAVE = 0;
        PRVARSumCount = 0;
        /****  End of the Additional Code for UnlimitedHand  **********************/

        mUH = UH.global;
    }


    /////////////////////////////////////////////////////
    /// UPDATE
    /////////////////////////////////////////////////////
    void Update()
    {
        //When finished the Calibration of the Sensors for the Hand Movements
        if (mAngleFlatAve > 30)
        {
            mPRCalib = false;
            mUH.updateAngleTempFlg = false;
            mUH.updatePhotoSensorsFlg = true;
        }
        //During the Calibration
        else if (30 >= mAngleFlatAve)
        {
            mUH.updateAngleTempFlg = true;        //update the forearm angle values
            mUH.updatePhotoSensorsFlg = true; //update the sensors' values of the hand movements
            handAngle[0] = mUH.UHAngle[0];    //get the forearm angle values
            handAngle[1] = mUH.UHAngle[1];
            handAngle[2] = mUH.UHAngle[2];

            //calc the average of the forearm angle values
            for (int i = 1; i < 10; i++)
            {
                mAngleFlat[i - 1] = mAngleFlat[i];
            }
            mAngleFlat[9] = handAngle[0];
            mAngleFlatAve = 0;
            for (int i = 0; i < 10; i++)
            {
                mAngleFlatAve += mAngleFlat[i];
            }
            mAngleFlatAve = mAngleFlatAve / 10;
        }


        uPR = mUH.readPhotoSensors();         //read the sensors' values of the hand movements

        //During the Calibration of the sensors' values of the hand movements
        if (mPRCalib)
        {
            for (int i = 0; i < PhotoSensorCount; i++)
            {
                uPRSum[i] += uPR[i];
                uPRAve[i] = uPRSum[i] / count;
            }
            count++;

            if (30 < count)
            {//reset the count
                for (int i = 0; i < PhotoSensorCount; i++)
                {
                    uPRSum[i] = uPRAve[i];
                }
                count = 1;
                mCalibCount++;
            }
        }
        //When finished the Calibration of the Sensors for the Hand Movements
        else if (!mPRCalib)
        {
            mPRVARSum = 0;

            //mPRVARSum += Mathf.Abs(uPR [0] - uPRAve [0]);
            mPRVARSum += Mathf.Abs(uPR[1] - uPRAve[1]);
            mPRVARSum += Mathf.Abs(uPR[2] - uPRAve[2]);
            mPRVARSum += Mathf.Abs(uPR[3] - uPRAve[3]);
            //mPRVARSum += Mathf.Abs(uPR [4] - uPRAve [4]);
            //mPRVARSum += Mathf.Abs(uPR [5] - uPRAve [5]);
            //mPRVARSum += Mathf.Abs(uPR [6] - uPRAve [6]);
            //mPRVARSum += Mathf.Abs(uPR [7] - uPRAve [7]);

            PRVARSumAVE += mPRVARSum;
            PRVARSumCount++;
            if (20 < PRVARSumCount)
            {
                PRVARSumAVE = PRVARSumAVE / PRVARSumCount;
                PRVARSumCount = 0;
                FIRE_THRESHOLD = PRVARSumAVE + 20;
            }
            //Debug.Log (mPRVARSum);
            if (mPRVARSum < FIRE_THRESHOLD)
            {
                // TODO
            }
            else if (FIRE_THRESHOLD <= mPRVARSum && mPRVARSum <= (FIRE_THRESHOLD + 100))
            {
                // TODO
                //fireHold = true;
                //if (gunshotFeedback && !isStimulating)
                //{
                //    uhand.stimulate(2);
                //    afterShotCount = 0;
                //    isStimulating = true;
                //}
            }
            if (isStimulating)
            {
                // TODO
                //afterShotCount++;
                //if (afterShotCount > 30)
                //{
                //    isStimulating = false;
                //}
            }
            /****  End of the Additional Code for UnlimitedHand  **********************/
        }
    }

    public bool IsFinishedCalibration()
    {
        return !mPRCalib;
    }

    public int getArmStatus()
    {
        int ret = 0;

        if (!mPRCalib)
        {
            if (uPR[5] > (uPRAve[5] + ArmStatusBorder) && uPR[6] > uPRAve[6])
            {
                ret = uPR[5] - (uPRAve[5] + ArmStatusBorder);
            }
            else if (uPR[5] > uPRAve[5] && uPR[6] > (uPRAve[6] + ArmStatusBorder))
            {
                ret = uPR[6] - (uPRAve[6] + ArmStatusBorder);
            }
            else if (uPR[5] < (uPRAve[5] - ArmStatusBorder))
            {
                ret = uPR[5] - (uPRAve[5] - ArmStatusBorder);
            }
            else if (uPR[6] < (uPRAve[6] - ArmStatusBorder))
            {
                ret = uPR[6] - (uPRAve[6] - ArmStatusBorder);
            }
        }

        return ret;
    }
}

