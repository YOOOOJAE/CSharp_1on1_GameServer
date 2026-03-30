using System;
using System.Collections.Generic;
using System.Text;

namespace RPGCommon
{
    //직업에 따른 수치설정
    public static class JobDataRepository
    {
        private static readonly Dictionary<JobType, JobData> _jobData = new Dictionary<JobType, JobData>()
        { { JobType.Normal,  new JobData(3, 0.8f, 7, 1.1f) },
            { JobType.Stamina,  new JobData(3, 1f, 9, 1.2f) },
            { JobType.Speed,  new JobData(3, 0.6f, 5, 0.9f) }};

        public static JobData GetStat(JobType type)
        {
            if(_jobData.TryGetValue(type, out JobData data))
            {
                return data;
            }
            return new JobData(1, 1, 1, 1);
        }
    }
}
