using Avalonia.Automation.Peers;

namespace ColorTextBlock.Avalonia
{
    /// <summary>
    /// The automation peer for CTextBlock.
    /// </summary>
    public class CTextBlockAutomationPeer : ControlAutomationPeer
    {
        public CTextBlockAutomationPeer(CTextBlock owner) : base(owner)
        { }

        public new CTextBlock Owner
            => (CTextBlock)base.Owner;

        protected override AutomationControlType GetAutomationControlTypeCore()
            => AutomationControlType.Text;

        protected override string? GetNameCore()
            => Owner.Text;

        protected override bool IsControlElementCore()
            => Owner.TemplatedParent is null && base.IsControlElementCore();
    }
}
