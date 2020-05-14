using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows;

using Juntendo.MedPhys.Esapi.IscFluenceOptimizer;
using System.IO;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            Patient currentPatient = context.Patient;
            PlanSetup currentPlanSetup = context.PlanSetup;
            Course currentCourse = context.Course;

            string planId = currentPlanSetup.Id;
            string courseId = currentCourse.Id;

            var parameterSettingViewModel = new ParameterSettingViewModel(planId, courseId, currentPatient);

            var parameterSettingView = new ParameterSettingView();
            parameterSettingView.DataContext = parameterSettingViewModel;

            var dialogResult = parameterSettingView.ShowDialog();
            if (!(bool)dialogResult)
            {
                return;
            }

            var newPlanId = parameterSettingViewModel.NewPlanId;
            if (currentCourse.PlanSetups.Where(p => p.Id == newPlanId).Count() >= 1)
            {
                MessageBox.Show($"Plan: {newPlanId} already exists \nExit Code");
                return;
            }

            var newPlanSetup = MakeIscPlan.CopyAndMakeNewIscPlan(parameterSettingViewModel.DoseThresholdPc, newPlanId, currentCourse, currentPlanSetup, parameterSettingViewModel.NumberOfSteps);
            MessageBox.Show($"Plan: {newPlanId} was successfully created");

            //MessageBox.Show($"Plan is selected.");

            //MessageBox.Show($"Before fluence shaping.");

            //var iscFluenceOptimizer = new IscFluenceOptimizer(currentPlanSetup, parameterSettingViewModel.DoseThresholdPc);
            //int numberOfBeams = currentPlanSetup.Beams.Count();

            ////MessageBox.Show($"Fluence is shaped.");
            //// var folderPath = GetDefaultFolderPath();

            //var folderPath = @"\\10.208.223.10\Eclipse\IscFluence";

            //// For Non-clinical Eclipse
            //var computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
            //if (computerName == "ECM516NC")
            //{
            //    folderPath = @"C:\Users\Admin\Desktop\IscFluence";
            //}

            ////MessageBox.Show($"Folder Path: {folderPath:s}");
            //foreach (BeamInfo beamInfo in iscFluenceOptimizer.BeamInfos)
            //{
            //    var f = beamInfo.IscFluence;
            //    var filePath = Path.Combine(folderPath, "Reduced" + f.BeamId + ".optimal_fluence");
            //    beamInfo.WriteReducedFluenceToFile(filePath);
            //}
        }
    }
}
