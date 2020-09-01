using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using SYE.Helpers;
using SYE.Services;
using Xunit;

namespace SYE.Tests.Helpers
{
    public class DynamicContentHelpersTests
    {
        //Setup shared context for all but main test
        private FormVM formContext = new FormVM()
        {
            FormName = "test Form",
            Id = "test-id",
            SubmissionData = new List<DataItemVM>()
            {
                new DataItemVM()
                {
                    Id = "formData1-id",
                    Value = "formData1-value"
                }
            },
            Pages = new List<PageVM>()
            {
                new PageVM()
                {
                    PageId = "Page1",
                    PageName = "Page One",
                    Questions = new List<QuestionVM>()
                    {
                        new QuestionVM()
                        {
                            QuestionId = "page1-question1",
                            Answer = "p1q1-answer"
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page1-question2"
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page1-duplicateQuestionId"
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page1-duplicateQuestionId"
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page1-blankAnswer",
                            Answer = ""
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page1-whitespaceAnswer",
                            Answer = "       "
                        },
                        new QuestionVM()
                        {
                        QuestionId = "page1-floatAnswer",
                        Answer = "1.1"
                        }
                    }
                },
                new PageVM()
                {
                    PageId = "Page2",
                    PageName = "Page Two",
                    Questions = new List<QuestionVM>()
                    {
                        new QuestionVM()
                        {
                            QuestionId = "page2-question1"
                        },
                        new QuestionVM()
                        {
                            QuestionId = "page2-question2"
                        },
                    },
                    DynamicContent = new DynamicContentVM()
                    {
                        Conditions = new List<ConditionVM>()
                        {
                            new ConditionVM()
                            {
                                DevNotes = "test condition 1a",
                                Priority = 1,
                                IfTrue = new IfTrueVM()
                                {
                                    OverrideQuestion = new QuestionVM()
                                    {
                                        Question = "Changed Question Content"
                                    },
                                    Conditions = new List<ConditionVM>()
                                    {
                                        new ConditionVM()
                                        {
                                            DevNotes = "test condition 1b",
                                            Priority = 1,
                                            IfTrue = new IfTrueVM()
                                            {
                                                OverrideQuestion = new QuestionVM()
                                                {
                                                    InputHeight = "Additional change to content"
                                                }
                                            },
                                            Logic = new LogicVM()
                                            {
                                                Source = "answer",
                                                Variable = "page1-question1",
                                                Operator = "equal",
                                                Answer = "p1q1-answer"
                                            }
                                        }
                                    }
                                },
                                Logic = new LogicVM()
                                {
                                    Source = "answer",
                                    Variable = "page1-question1",
                                    Operator = "equal",
                                    Answer = "p1q1-answer"
                                }
                            },
                            new ConditionVM()
                            {
                                DevNotes = "test condition 2",
                                Priority = 2,
                                IfTrue = new IfTrueVM()
                                {
                                    OverridePage = new PageVM()
                                    {
                                        PageTitle = "Changed Page Content"
                                    }
                                },
                                Logic = new LogicVM()
                                {
                                    Source = "answer",
                                    Variable = "page1-question1",
                                    Operator = "equal",
                                    Answer = "p1q1-answer"
                                }
                            }
                        }
                    }
                }
            }
        };

        //Tests
        [Fact]
        public void GetQuestionShould_ErrorIfQuestionNotFound()
        {
            //Arrange
            var questionId = "not an ID";

            //Act + Assert
            var ex = Assert.Throws<Exception>(() => 
                formContext.GetQuestion(questionId));
            ex.Message.Should().Contain("QuestionId cannot be found");
        }
        
        [Fact]
        public void GetQuestionShould_ErrorIfDuplicateIds()
        {
            //Arrange
            var questionId = "page1-duplicateQuestionId";

            //Act + Assert
            var ex = Assert.Throws<Exception>(() => 
                formContext.GetQuestion(questionId));
            ex.Message.Should().Contain("Duplicate questionIds found, unable to resolve match");
        }
        
        [Fact]
        public void GetQuestionShould_ReturnQuestionVM()
        {
            //Arrange
            var questionId = "page1-question1";
            var expectedResult = new QuestionVM()
            {
                QuestionId = "page1-question1",
                Answer = "p1q1-answer"
            };

            //Act
            var sut = formContext.GetQuestion(questionId);

            //Assert
            sut.Should().BeEquivalentTo(expectedResult);
        }

        
        [Fact]
        public void EvaluateConditionShould_ErrorIfNoAnswerSupplied()
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Answer = null
                }
            };

            //Act + Assert
            var ex = Assert.Throws<NullReferenceException>(() =>
                DynamicContentHelpers.EvaluateCondition(condition, formContext));
            ex.Message.Should().Contain("Object reference not set to an instance of an object");
        }
        
        [Fact]
        public void EvaluateConditionShould_ErrorIfVariableSourceUnrecognised()
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = "not-a-source",
                    Answer = ""
                }
            };

            //Act + Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                DynamicContentHelpers.EvaluateCondition(condition, formContext));
            ex.Message.Should().Contain("Unrecognised variable source type");
        }
        
        [Fact]
        public void EvaluateConditionShould_ErrorIfVariableNotFound()
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = "formData",
                    Variable = "not-a-formData-id",
                    Answer = ""
                }
            };

            //Act + Assert
            var ex = Assert.Throws<Exception>(() => DynamicContentHelpers.EvaluateCondition(condition, formContext));
            ex.Message.Should().Contain("Unable to resolve dynamic content variable");
        }
        
        [Fact]
        public void EvaluateConditionShould_ErrorIfOperatorUnrecognised()
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = "answer",
                    Variable = "page1-question1",
                    Operator = "not-an-operator",
                    Answer = ""
                }
            };

            //Act + Assert
            var ex = Assert.Throws<ArgumentException>(() => DynamicContentHelpers.EvaluateCondition(condition, formContext));
            ex.Message.Should().Contain("Unrecognised logic operator");
        }
        
        [Theory]
        [InlineData("page1-question1", "greater_than", "1.1")]
        [InlineData("page1-floatAnswer", "greater_than", "not-a-float")]
        [InlineData("page1-question1", "less_than", "1.1")]
        [InlineData("page1-floatAnswer", "less_than", "not-a-float")]
        public void EvaluateConditionShould_ErrorIf_OperatorIsGreaterOrLessThan_AndVariableOrAnswerCannotBeParsedToFloat(string variable, string Operator, string answer)
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = "answer",
                    Variable = variable,
                    Operator = Operator,
                    Answer = answer
                }
            };

            //Act + Assert
            var ex = Assert.Throws<ArgumentException>(() => DynamicContentHelpers.EvaluateCondition(condition, formContext));
            ex.Message.Should().Contain("cannot be parsed to float");
        }
        
        [Theory]
        [InlineData("answer", "page1-question1", "equal", "p1q1-answer")]
        [InlineData("answer", "page1-question1", "equal", "p1q1-answer|or-not-answer")]

        [InlineData("answer", "page1-question1", "not_equal", "not-the-data")]
        [InlineData("answer", "page1-question1", "not_equal", "not-the-data|also-not-the-data")]

        [InlineData("answer", "page1-question1", "contains", "p1q1")]
        [InlineData("answer", "page1-question1", "contains", "p1q1|not-this")]

        [InlineData("answer", "page1-question1", "contains_all", "p1q1|answer")]

        [InlineData("answer", "page1-blankAnswer", "is_empty", "")]
        [InlineData("answer", "page1-whitespaceAnswer", "is_empty", "")]

        [InlineData("answer", "page1-floatAnswer", "greater_than", "1")]
        [InlineData("answer", "page1-floatAnswer", "greater_than", "1.000")]
        [InlineData("answer", "page1-floatAnswer", "greater_than_or_equal", "1.1")]
        [InlineData("answer", "page1-floatAnswer", "greater_than_or_equal", "1.100")]
        [InlineData("answer", "page1-floatAnswer", "less_than", "2")]
        [InlineData("answer", "page1-floatAnswer", "less_than", "2.0000")]
        [InlineData("answer", "page1-floatAnswer", "less_than_or_equal", "1.1")]
        [InlineData("answer", "page1-floatAnswer", "less_than_or_equal", "1.1000")]
        public void EvaluateConditionShould_ReturnTrue(string source, string variable, string Operator, string answer)
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = source,
                    Variable = variable,
                    Operator = Operator,
                    Answer = answer
                }
            };

            //Act
            var sut = DynamicContentHelpers.EvaluateCondition(condition, formContext);

            //Assert
            sut.Should().BeTrue();
        }
        
        [Theory]
        [InlineData("answer", "page1-question1", "equal", "not-the-data")]
        [InlineData("answer", "page1-question1", "equal", "not-the-data|also-not-the-data")]

        [InlineData("answer", "page1-question1", "not_equal", "p1q1-answer")]
        [InlineData("answer", "page1-question1", "not_equal", "p1q1-answer|or-not-answer")]

        [InlineData("answer", "page1-question1", "contains", "not-this")]
        [InlineData("answer", "page1-question1", "contains", "not-this|or-this")]

        [InlineData("answer", "page1-question1", "contains_all", "not-this|or-this")]
        [InlineData("answer", "page1-question1", "contains_all", "p1q1|but-not-this")]

        [InlineData("answer", "page1-question1", "is_empty", "")]
        [InlineData("answer", "page1-question1", "is_empty", "")]

        [InlineData("answer", "page1-floatAnswer", "greater_than", "2")]
        [InlineData("answer", "page1-floatAnswer", "greater_than", "2.000")]
        [InlineData("answer", "page1-floatAnswer", "greater_than_or_equal", "2")]
        [InlineData("answer", "page1-floatAnswer", "greater_than_or_equal", "2.000")]
        [InlineData("answer", "page1-floatAnswer", "less_than", "1")]
        [InlineData("answer", "page1-floatAnswer", "less_than", "1.0000")]
        [InlineData("answer", "page1-floatAnswer", "less_than_or_equal", "1")]
        [InlineData("answer", "page1-floatAnswer", "less_than_or_equal", "1.0000")]
        public void EvaluateConditionShould_ReturnFalse(string source, string variable, string Operator, string answer)
        {
            //Arrange
            var condition = new ConditionVM()
            {
                Logic = new LogicVM()
                {
                    Source = source,
                    Variable = variable,
                    Operator = Operator,
                    Answer = answer
                }
            };

            //Act
            var sut = DynamicContentHelpers.EvaluateCondition(condition, formContext);

            //Assert
            sut.Should().BeFalse();
        }

        
        [Fact]
        public void HandleDynamicContentShould_ReturnSamePageIf_NoDynamicContent()
        {
            //Arrange
            var myPage = new PageVM()
            {
                PageId = "TestPage",
                PageName = "Test page",
                Questions = new List<QuestionVM>()
                {
                    new QuestionVM()
                    {
                        QuestionId = "Test Question"
                    }
                }
            };
            
            //Act
            var sut = DynamicContentHelpers.HandleDynamicContent(myPage, formContext);
            
            //Assert
            sut.Should().BeEquivalentTo(myPage);
        }

        [Fact]
        public void HandleDynamicContentShould_ReturnUnchangedPageIfItErrors()
        {
            //Arrange
            var myPage = new PageVM()
            {
                PageId = "TestPage",
                PageName = "",
                DynamicContent = new DynamicContentVM()
                {
                    DefaultAction = "This will return an error"
                },
                Questions = new List<QuestionVM>()
                {
                    new QuestionVM()
                    {
                        QuestionId = "Test Question"
                    }
                }
            };

            //Act
            var sut = DynamicContentHelpers.HandleDynamicContent(myPage, formContext);

            //Assert
            sut.Should().BeEquivalentTo(myPage);
        }
        
        [Fact]
        public void HandleDynamicContentShouldApplyExpectedChanges()
        {
            //Arrange - this is a complex test so it uses a new pseudo-schema held entirely in the below variables:
            var referenceForm = new FormVM()
            {
                SubmissionData = new List<DataItemVM>()
                {
                    new DataItemVM()
                    {
                        Id = "InputDataItem-id",
                        Value = "InputDataItem-value"
                    }
                },
                Pages = new List<PageVM>()
                {
                    new PageVM()
                    {
                        PageId = "ReferencePage",
                        PageName = "Reference Page",
                        Questions = new List<QuestionVM>()
                        {
                            new QuestionVM()
                            {
                                QuestionId = "Ref-Id",
                                Question = "Ref-Question",
                                DocumentOrder = 2,
                                InputType = "Ref-Type",
                                Answer = "Ref-Answer"
                            }
                        }
                    }
                }
            };

            var questionCondition = new ConditionVM()
            {
                DevNotes = "Test condition set",
                Logic = new LogicVM()
                {
                    Source = "answer",
                    Variable = "Ref-Id",
                    Operator = "equal",
                    Answer = "Ref-Answer"
                },
                Priority = 1,
                IfTrue = new IfTrueVM()
                {
                    OverrideQuestion = new QuestionVM()
                    {
                        Answer = "Edited by question dynamic content",
                        AdditionalText = "Should-be-overwritten-by-subcondition",
                        ShortQuestion = "Added by question dynamic content"
                    },
                    Conditions = new List<ConditionVM>()
                    {
                        new ConditionVM()
                        {
                            DevNotes = "Test subcondition",
                            Logic = new LogicVM()
                            {
                                Source = "answer",
                                Variable = "Ref-Id",
                                Operator = "equal",
                                Answer = "Ref-Answer"
                            },
                            Priority = 1,
                            IfTrue = new IfTrueVM()
                            {
                                OverrideQuestion = new QuestionVM()
                                {
                                    AdditionalText = "Overwritten field by question subcondition",
                                    InputHeight = "Field added by subcondition"
                                }
                            }
                        }
                    }
                }
            };

            var pageCondition = new ConditionVM()
            {

                DevNotes = "Test condition set for page-level",
                Priority = 1,
                Logic = new LogicVM()
                {
                    Source = "formData",
                    Variable = "InputDataItem-id",
                    Operator = "equal",
                    Answer = "InputDataItem-value"
                },
                IfTrue = new IfTrueVM()
                {
                    OverridePage = new PageVM()
                    {
                        NextPageId = "Added Field",
                        PageName = "Edited by page dynamic content"
                    }
                }

            };

            var inputPage = new PageVM()
            {
                PageId = "TestPage",
                PageName = "Test page",
                DynamicContent = new DynamicContentVM()
                {
                    Conditions = new List<ConditionVM>() { pageCondition }
                },
                Questions = new List<QuestionVM>()
                {
                    new QuestionVM()
                    {
                        QuestionId = "Test Question not-overwritten",
                        DynamicContent = new DynamicContentVM()
                        {
                            Conditions = new List<ConditionVM>(){questionCondition}
                        },
                    }
                }
            };

            var expectedOutput = new PageVM()
            {
                PageId = "TestPage",
                PageName = "Edited by page dynamic content",
                NextPageId = "Added Field",
                DynamicContent = new DynamicContentVM()
                {
                    Conditions = new List<ConditionVM>(){pageCondition}
                },
                Questions = new List<QuestionVM>()
                {
                    new QuestionVM()
                    {
                        QuestionId = "Test Question not-overwritten",
                        DynamicContent = new DynamicContentVM()
                        {
                            Conditions = new List<ConditionVM>(){questionCondition}
                        },
                        Answer = "Edited by question dynamic content",
                        AdditionalText = "Overwritten field by question subcondition",
                        ShortQuestion = "Added by question dynamic content",
                        InputHeight = "Field added by subcondition"

                    }
                }
            };

            //Act
            var sut = inputPage;

            sut.HandleDynamicContent(referenceForm);

            //Assert
            sut.Should().BeEquivalentTo(expectedOutput);
        }

        [Fact]
        public void HandleDynamicContentShould_NotHandleErrors()
        {
            //Arrange

            //Act

            //Assert
        }
        
        [Fact]
        public void ApplyDynamicContentShould_NotHandleErrors()
        {
            //Arrange

            //Act

            //Assert
        }
        
        [Fact]
        public void EvaluateCondition_NotHandleErrors()
        {
            //Arrange

            //Act

            //Assert
        }
        
        [Fact]
        public void GetQuestionShould_NotHandleErrors()
        {
            //Arrange

            //Act

            //Assert
        }
    }
}
