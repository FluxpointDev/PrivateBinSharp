namespace PrivateBinSharp.Crypto.asn1;

/**
 * Marker interface for CHOICE objects - if you implement this in a roll-your-own
 * object, any attempt to tag the object implicitly will convert the tag to an
 * explicit one as the encoding rules require.
 * <p>
 * If you use this interface your class should also implement the getInstance
 * pattern which takes a tag object and the tagging mode used. 
 * </p>
 */
// TODO[api] Add method to Report the smallest tag that can appear (for use with CER encoding rules).
internal interface IAsn1Choice
{
	// marker interface
}
