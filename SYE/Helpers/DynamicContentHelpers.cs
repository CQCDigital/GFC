using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GDSHelpers.Models.FormSchema;

namespace SYE.Helpers
{
    public static class DynamicContentHelpers
    {
        /// <summary>
        /// Handle logic for dynamic content within a page, overriding supplied content as required
        /// </summary>
        /// <param name="page">The page with potential dynamic content</param>
        /// <param name="formContext">Form the question is part of</param>
        /// <returns></returns>
        public static PageVM HandleDynamicContent(this PageVM page, FormVM formContext)
        {
            var defaultPage = page;
            
            try
            {
                if (page.DynamicContent != null)
                {
                    var conditions = page.DynamicContent.Conditions;
                    page.ApplyDynamicContent(conditions, formContext);
                }

                foreach (var question in page.Questions)
                {
                    if (question.DynamicContent != null)
                    {
                        var conditions = question.DynamicContent.Conditions;
                        question.ApplyDynamicContent(conditions, formContext);
                    }
                }
            }
            catch
            {
                return defaultPage;
            }

            return page;
        }


        //RECURSIVE METHOD
        //TODO: Maybe change to only output the list of changes, with an id?. But to do this, need 'overwrite' code (separate method)?
        /// <summary>
        /// RECURSIVE: This amends a page or question based on a set of supplied conditions and a form context. Question/page and conditions MUST be supplied separately, as any question/page may have several conditions objects
        /// </summary>
        /// <param name="outputObject"></param>
        /// <param name="inputConditions"></param>
        /// <param name="formContext"></param>
        /// <returns></returns>
        public static T ApplyDynamicContent<T>(this T outputObject, IEnumerable<ConditionVM> inputConditions, FormVM formContext)
        {
            var result = false;
            T output = outputObject;

            var conditions = inputConditions.OrderBy(c => c.Priority);
            foreach (ConditionVM condition in conditions) //Loop over conditions at this 'level' in priority order
            {
                //If true, do stuff; if false, try next condition
                if (EvaluateCondition(condition, formContext))
                {
                    if (condition.IfTrue.OverridePage != null)
                    {
                        //it's a page
                        var overrideContent = condition.IfTrue.OverridePage;
                        //Get the type, and run through the public properties
                        foreach (PropertyInfo propertyInfo in overrideContent.GetType().GetProperties())
                        {
                            //If the value for this property in the overrides object is not null... (i.e. it has been defined)
                            if (propertyInfo.GetValue(overrideContent) != null)
                            {
                                //...set the output value to this value.
                                propertyInfo.SetValue(output, propertyInfo.GetValue(overrideContent));
                            }
                        }
                    }
                    else
                    {
                        //it's a question
                        var overrideContent = condition.IfTrue.OverrideQuestion;
                        //Get the type, and run through the public properties
                        foreach (PropertyInfo propertyInfo in overrideContent.GetType().GetProperties())
                        {
                            //If the value for this property in the overrides object is not null... (i.e. it has been defined)
                            if (propertyInfo.GetValue(overrideContent) != null)
                            {
                                //...set the output value to this value.
                                propertyInfo.SetValue(output, propertyInfo.GetValue(overrideContent));
                            }
                        }
                    }

                    ////Apply any overrides at this stage
                    //if (overrideContent != null)
                    //{
                    //    //Get the type, and run through the public properties
                    //    foreach (PropertyInfo propertyInfo in overrideContent.GetType().GetProperties())
                    //    {
                    //        //If the value for this property in the overrides object is not null... (i.e. it has been defined)
                    //        if (propertyInfo.GetValue(overrideContent) != null)
                    //        {
                    //            //...set the output value to this value.
                    //            propertyInfo.SetValue(output, propertyInfo.GetValue(overrideContent));
                    //        }
                    //    }
                    //}

                    //TODO: Finish 'other action' logic, e.g. 'substitute for entirely different question from schema'
                    var otherActions = condition.IfTrue.OtherAction; 
                    if (otherActions != null)
                    {
                        ResolveCustomAction(otherActions);
                    }

                    //If there are sub-conditions, resolve these recursively
                    var subConditions = condition.IfTrue.Conditions?.OrderBy(c => c.Priority); 
                    if (subConditions != null)
                    {
                        //output = ApplyQuestionLogic(output, subConditions, formContext);
                        output.ApplyDynamicContent(subConditions, formContext);
                    }

                    //TODO: handle application order in case of multiple successful conditions
                    //return output;
                }

                //By default will loop to the next condition with result currently == false
            }

            return output;
        }


        /// <summary>
        /// Evaluates a condition against source data in formVM and returns true or false
        /// </summary>
        /// <param name="condition">Logic source</param>
        /// /// <param name="formContext">Data source</param>
        /// <returns></returns>
        public static bool EvaluateCondition(ConditionVM condition, FormVM formContext)
        {
            var result = false;

            //Load in condition parameters
            var variableName = condition.Logic.Variable;
            var answer = condition.Logic.Answer;
            //var answerArray = condition.Logic.AnswerArray;
            var answerArray = answer.Split('|');

            //Find variable in session formContext
            string variable = null;
            switch (condition.Logic.Source)
            {
                case "answer":
                    variable = formContext.GetQuestion(variableName).Answer;
                    break;
                case "formData":
                    if (formContext.SubmissionData == null)
                    {
                        throw new Exception("No submissionData found");
                    }
                    variable = formContext.SubmissionData.Where(d => d.Id == variableName)
                        .Select(d => d.Value).FirstOrDefault();
                    break;
                default:
                    throw new ArgumentException($"Unrecognised variable source type {condition.Logic.Source} supplied by form");
                    break;
            }

            if (variable == null) //Whitespace is now explicitly allowed, to allow for checking of empty answers
            {
                throw new Exception($"Unable to resolve dynamic content variable {variableName}");
            }

            //Set result to true if condition is met | CONSIDER making all of these individual methods
            switch (condition.Logic.Operator)
            {
                case "equal":
                    result = answerArray.Any(variable.Equals);  //(variable == answer);
                    break;
                case "is_empty":
                    result = string.IsNullOrWhiteSpace(variable);
                    break;
                case "not_equal":
                    result = !(answerArray.Any(variable.Equals));  //(variable != answer);
                    break;
                //TODO: Consider inclusion of 'includes' and other string functions
                case "contains":
                    result = answerArray.Any(variable.Contains);
                    break;
                case "contains_all":
                    result = answerArray.All(variable.Contains);
                    break;
                case "greater_than":
                    try
                    {
                        result = (float.Parse(variable) > float.Parse(answer));
                    }
                    catch
                    {
                        throw new ArgumentException($"Variable or answer {variable} cannot be parsed to float");
                    }
                    break;
                case "greater_than_or_equal":
                    try
                    {
                        result = (float.Parse(variable) >= float.Parse(answer));
                    }
                    catch
                    {
                        throw new ArgumentException($"Variable or answer {variable} cannot be parsed to float");
                    }
                    break;
                case "less_than":
                    try
                    {
                        result = (float.Parse(variable) < float.Parse(answer));
                    }
                    catch
                    {
                        throw new ArgumentException($"Variable or answer {variable} cannot be parsed to float");
                    }
                    break;
                case "less_than_or_equal":
                    try
                    {
                        result = (float.Parse(variable) <= float.Parse(answer));
                    }
                    catch
                    {
                        throw new ArgumentException($"Variable or answer {variable} cannot be parsed to float");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unrecognised logic operator {condition.Logic.Operator} supplied by form");
                    break;
            }

            return result;
        }


        /// <summary>
        /// Gets a question. Throws exceptions if there are multiple matches or question cannot be found.
        /// </summary>
        /// <param name="form">Form containing question</param>
        /// <param name="questionId">Question we need</param>
        /// <returns></returns>
        public static QuestionVM GetQuestion(this FormVM form, string questionId)
        {
            var matches = new List<QuestionVM>();

            foreach (PageVM page in form.Pages)
            {
                matches.AddRange(page.Questions.Where(q => q.QuestionId == questionId));
            }
            if (matches.Count > 1)
            {
                throw new Exception("Duplicate questionIds found, unable to resolve match");
            }
            else if (matches.Count == 0)
            {
                throw new Exception("QuestionId cannot be found");
            }

            return matches.FirstOrDefault();
        }


        /// <summary>
        /// Placeholder: resolve any supplied custom actions
        /// </summary>
        /// <param name="action">Action parameter</param>
        public static void ResolveCustomAction(string action)
        {
            //do nothing
            //throw new NotImplementedException();
            //
            //switch (action)
            //{
            //    case "load_question":
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}
