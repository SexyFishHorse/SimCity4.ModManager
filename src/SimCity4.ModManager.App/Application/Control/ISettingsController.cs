using System.Collections.Generic;
using System.Drawing;

namespace SimCity4.ModManager.App.Application.Control;

public interface ISettingsController
{
    bool ValidateGameLocationPath(string path);

    void CheckMainFolder();

    string SearchForGameLocation();

    IEnumerable<string> GetInstalledLanguages();

    IList<Bitmap> GetWallpapers();
}