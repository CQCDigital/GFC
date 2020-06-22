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
using SYE.Helpers;
using SYE.Services;
using SYE.ViewModels;
using Xunit;

namespace SYE.Tests.Helpers
{
    public class PageHelperTests
    {
        private IPageHelper pageHelper = new PageHelper();

        [Fact]
        public void GetGetPreviousPage_should_return_default_page()
        {
            //arrange
            var previousPage = "previouspage";
            var thisPage = new PageVM
            {
                PageId = "what-you-want-to-tell-us-about",
                PreviousPages = new List<PreviousPageVM>
                {
                    new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                    new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                    new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
                },
                Questions = new List<QuestionVM> { new QuestionVM { Answer = "answer1" } },
                PreviousPageId = previousPage
            };
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123", ServiceNotFoundPage = "456", DefaultBackLink = previousPage };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            //act
            var result = pageHelper.GetPreviousPage(thisPage, mockSessionService.Object, options, mockUrlHelper.Object, false);
            //assert
            result.Should().BeEquivalentTo(previousPage);
        }
        [Fact]
        public void GetGetPreviousPage_should_return_first_previous_page()
        {
            //arrange
            var previousPage = "previouspage";
            var thisPage = new PageVM
            {
                PageId = "123987",
                PreviousPages = new List<PreviousPageVM>
                {
                    new PreviousPageVM {PageId = previousPage, QuestionId = "", Answer = ""}
                },
                Questions = new List<QuestionVM> { new QuestionVM { Answer = "answer1" } },
            };
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));

            ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123", ServiceNotFoundPage = "456", DefaultBackLink = "789" };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            //act
            var result = pageHelper.GetPreviousPage(thisPage, mockSessionService.Object, options, mockUrlHelper.Object, false);
            //assert
            result.Should().ContainAny(previousPage);
        }
        [Fact]
        public void GetGetPreviousPage_should_return_selected_previous_page()
        {
            //arrange
            var previousPage = "previouspage";
            var thisPage = new PageVM
            {
                PageId = "123987",
                PreviousPages = new List<PreviousPageVM>
                {
                    new PreviousPageVM {PageId = previousPage, QuestionId = "1", Answer = "answer1"},
                    new PreviousPageVM {PageId = "id2", QuestionId = "2", Answer = ""},
                    new PreviousPageVM {PageId = "id3", QuestionId = "3", Answer = ""}
                },
                Questions = new List<QuestionVM> { new QuestionVM { Answer = "answer1" } },
            };
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {QuestionId = "1", Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM { QuestionId = "234", Answer = "answer2"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM { QuestionId = "567", Answer = "answer3"}}
                    }
                }
            };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns((UrlActionContext uac) =>
                $"{uac.Controller}/{uac.Action}#{uac.Fragment}?" + string.Join("&",
                    new RouteValueDictionary(uac.Values).Select(p => p.Key + "=" + p.Value)));

            ApplicationSettings appSettings = new ApplicationSettings() { FormStartPage = "123", ServiceNotFoundPage = "456", DefaultBackLink = "789" };
            IOptions<ApplicationSettings> options = Options.Create(appSettings);

            //act
            var result = pageHelper.GetPreviousPage(thisPage, mockSessionService.Object, options, mockUrlHelper.Object, false);
            //assert
            result.Should().ContainAny(previousPage);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CheckPageHistory_with_empty_referer_should_return_false(bool flag)
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            var candidatePages = new List<PreviousPageVM>();
            var pageVM = new PageVM { PreviousPages = candidatePages };
            //act
            var result = pageHelper.CheckPageHistory(pageVM, "", flag, mockSessionService.Object, null);
            //assert
            result.Should().BeFalse();
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CheckPageHistory_with_candidates_should_return_true(bool flag)
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM{Answer = "qwe", QuestionId = "1", AnswerLogic = new List<AnswerLogicVM>{new AnswerLogicVM{NextPageId = "what-you-want-to-tell-us-about" } }},
                new QuestionVM{Answer = "qwe", QuestionId = "2", AnswerLogic = new List<AnswerLogicVM>{new AnswerLogicVM{NextPageId = "did-you-hear-about-this-form-from-a-charity" } }},
                new QuestionVM{Answer = "qwe", QuestionId = "3", AnswerLogic = new List<AnswerLogicVM>{new AnswerLogicVM{NextPageId = "give-your-feedback" } }},
            };
            var navOrderList = new List<string> { "what-you-want-to-tell-us-about", "did-you-hear-about-this-form-from-a-charity", "give-your-feedback" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);
            var pageVM = new PageVM { PreviousPages = candidatePages, Questions = questions };
            //act

            var result = pageHelper.CheckPageHistory(pageVM, "check-your-answers", flag, mockSessionService.Object, null);

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CheckPageHistory_with_candidates_with_one_unanswered_question_should_return_false()
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = ""}}
                    }
                }
            };

            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM
                    {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string>
                {"what-you-want-to-tell-us-about", "did-you-hear-about-this-form-from-a-charity", "give-your-feedback"};
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PreviousPages = candidatePages, Questions = questions };
            //act

            var result = pageHelper.CheckPageHistory(pageVM, "check-your-answers", true, mockSessionService.Object, null);

            //assert
            result.Should().BeFalse();
        }
        [Fact]
        public void CheckPageHistory_with_candidates_but_non_answered_questions_should_return_false()
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                    {
                        new PageVM
                        {
                            PageId = "what-you-want-to-tell-us-about",
                            Questions = new List<QuestionVM> {new QuestionVM {Answer = ""}}
                        },
                        new PageVM
                        {
                            PageId = "did-you-hear-about-this-form-from-a-charity",
                            Questions = new List<QuestionVM> {new QuestionVM {Answer = ""}}
                        },
                        new PageVM
                        {
                            PageId = "give-your-feedback",
                            Questions = new List<QuestionVM> {new QuestionVM {Answer = ""}}
                        }
                    }
            };
            var candidatePages = new List<PreviousPageVM>
                {
                    new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                    new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                    new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
                };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about", "did-you-hear-about-this-form-from-a-charity", "give-your-feedback" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PreviousPages = candidatePages, Questions = questions };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, "check-your-answers", true, mockSessionService.Object, null);

            //assert
            result.Should().BeFalse();
        }
        [Fact]
        public void CheckPageHistory_with_candidates_and_any_visited_pages_should_return_true()
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PreviousPages = candidatePages, Questions = questions };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, "check-your-answers", false, mockSessionService.Object, null);

            //assert
            result.Should().BeTrue();
        }
        [Theory]
        [InlineData("", true)]
        [InlineData("", false)]
        [InlineData("my-unknown-page", true)]
        [InlineData("my-unknown-page", false)]
        public void CheckPageHistory_with_unknown_referer_should_return_false(string referer, bool flag)
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},

                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };
            var navOrderList = new List<string>();
            if (flag)
            {
                navOrderList.Add("some-page-id");
                navOrderList.Add("another-page-id");
            }

            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PageId = "this-is-a-page-id", PreviousPages = candidatePages, Questions = questions, NextPageId = "previous-page-id" };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, referer, flag, mockSessionService.Object, null);

            //assert
            result.Should().BeFalse();
        }
        [Fact]
        public void CheckPageHistory_with_external_referer_should_return_true()
        {
            //arrange
            var externalStartPage = "my-external-page";
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PageId = "this-is-a-page-id", PreviousPages = candidatePages, Questions = questions, NextPageId = "next-page" };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, externalStartPage, false, mockSessionService.Object, externalStartPage);

            //assert
            result.Should().BeTrue();
        }
        [Fact]
        public void CheckPageHistory_with_invalid_external_referer_should_return_false()
        {
            //arrange
            var externalStartPage = "my-external-page";
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM {PageId = "this-is-a-page-id", PreviousPages = candidatePages, Questions = questions, NextPageId = "next-page" };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, "my-invalid-referer", false, mockSessionService.Object, externalStartPage);

            //assert
            result.Should().BeFalse();
        }
        [Fact]
        public void CheckPageHistory_with_null_external_referer_should_return_true()
        {
            //arrange
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PreviousPages = candidatePages, Questions = questions, NextPageId = "next-page" };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, "what-you-want-to-tell-us-about", false, mockSessionService.Object, null);

            //assert
            result.Should().BeTrue();
        }
        [Fact]
        public void CheckPageHistory_with_null_external_referer_should_return_false()
        {
            //arrange
            var externalStartPage = "my-external-page";
            var mockSessionService = new Mock<ISessionService>();
            FormVM formVm = new FormVM
            {
                Pages = new List<PageVM>
                {
                    new PageVM
                    {
                        PageId = "what-you-want-to-tell-us-about",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "did-you-hear-about-this-form-from-a-charity",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    },
                    new PageVM
                    {
                        PageId = "give-your-feedback",
                        Questions = new List<QuestionVM> {new QuestionVM {Answer = "answer1"}}
                    }
                }
            };
            var candidatePages = new List<PreviousPageVM>
            {
                new PreviousPageVM {PageId = "what-you-want-to-tell-us-about", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "did-you-hear-about-this-form-from-a-charity", QuestionId = "", Answer = ""},
                new PreviousPageVM {PageId = "give-your-feedback", QuestionId = "", Answer = ""}
            };
            var questions = new List<QuestionVM>
            {
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "1",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "what-you-want-to-tell-us-about"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "2",
                    AnswerLogic = new List<AnswerLogicVM>
                        {new AnswerLogicVM {NextPageId = "did-you-hear-about-this-form-from-a-charity"}}
                },
                new QuestionVM
                {
                    Answer = "qwe", QuestionId = "3",
                    AnswerLogic = new List<AnswerLogicVM> {new AnswerLogicVM {NextPageId = "give-your-feedback"}}
                },
            };

            var navOrderList = new List<string> { "what-you-want-to-tell-us-about" };
            mockSessionService.Setup(x => x.GetFormVmFromSession()).Returns(formVm);
            mockSessionService.Setup(x => x.GetNavOrder()).Returns(navOrderList);

            var pageVM = new PageVM { PageId = "this-is-a-page-id", PreviousPages = candidatePages, Questions = questions, NextPageId = "next-page" };

            //act
            var result = pageHelper.CheckPageHistory(pageVM, "my-invalid-referer", false, mockSessionService.Object, null);

            //assert
            result.Should().BeFalse();
        }
    }
}
