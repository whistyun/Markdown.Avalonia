using ApprovalTests;
using ApprovalTests.Reporters;
using Avalonia.Controls;
using Markdown.Avalonia.Mermaid;
using NUnit.Framework;
using UnitTest.Base;
using UnitTest.Base.Utils;

namespace UnitTest.Mermaid
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTestMermaid : UnitTestBase
    {
        static UnitTestMermaid()
        {
            MermaidBlockHandler.EnableMermaidRendering = false;
        }

        [Test]
        [RunOnUI]
        public void Transform_givenFlowcharts_generatesExpectedResult()
        {
            var text = Util.LoadText("Flowcharts.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

		[Test]
		[RunOnUI]
		public void Transform_givenSequenceDiagrams_generatesExpectedResult()
		{
			var text = Util.LoadText("SequenceDiagrams.md");
			var markdown = new Markdown.Avalonia.Markdown();
			markdown.UseMermaid();
			var result = markdown.Transform(text);
			Approvals.Verify(Util.AsXaml(result));
		}

        [Test]
        [RunOnUI]
        public void Transform_givenClassDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("ClassDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenEntityRelationshipDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("EntityRelationshipDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenGanttDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("GanttDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenGitGraphDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("GitGraphDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenMindmap_generatesExpectedResult()
        {
            var text = Util.LoadText("Mindmap.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenPieChartDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("PieChartDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenQuadrantChart_generatesExpectedResult()
        {
            var text = Util.LoadText("QuadrantChart.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenRequirementDiagram_generatesExpectedResult()
        {
            var text = Util.LoadText("RequirementDiagram.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenStateDiagrams_generatesExpectedResult()
        {
            var text = Util.LoadText("StateDiagrams.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenTimelineDiagram_generatesExpectedResult()
        {
            var text = Util.LoadText("TimelineDiagram.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }

        [Test]
        [RunOnUI]
        public void Transform_givenUserJourneyDiagram_generatesExpectedResult()
        {
            var text = Util.LoadText("UserJourneyDiagram.md");
            var markdown = new Markdown.Avalonia.Markdown();
            markdown.UseMermaid();
            var result = markdown.Transform(text);
            Approvals.Verify(Util.AsXaml(result));
        }
    }
}
