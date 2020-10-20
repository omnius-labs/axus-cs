using System.Threading.Tasks;
using Omnius.Xeus.Api;
using Omnius.Xeus.Api.Models;

namespace Omnius.Xeus.Ui.Desktop.Interactors
{
    class FileSearchInteractor
    {
        private readonly IXeusService _xeusService;

        public FileSearchInteractor(IXeusService xeusService)
        {
            _xeusService = xeusService;
        }

        public async Task Run()
        {
            var param = new ExportWantDeclaredMessageParam();
            await _xeusService.ExportWantDeclaredMessageAsync(param);
        }
    }
}
