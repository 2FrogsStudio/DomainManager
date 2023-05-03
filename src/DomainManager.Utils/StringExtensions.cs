namespace DomainManager;

public static class StringExtensions {
    public static bool TryGetDomainFromInput(this string input, out string domain) {
        domain = input;
        return true;
        // if (!input.Contains(Uri.SchemeDelimiter)) {
        //     input = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, input);
        // }
        //
        // domain = new Uri(input).Host;
        // return true;
    }
}