# Solr storage for audio fingerprinting framework in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

_soundfingerprinting.solr_ is Solr backend implementation for [soundfingerprinting](https://github.com/AddictedCS/soundfingerprinting) framework. It is very resilient, fast, non-relational database, which allows you to scale audio fingerprint storage according to you business needs.

[![Build Status](https://travis-ci.org/AddictedCS/soundfingerprinting.solr.png)](https://travis-ci.org/AddictedCS/soundfingerprinting.solr)

### How to use
`SoundFingerprinting.Solr` uses `SorlNet` in order to connect to actual solr instances. Please provide connection string for both cores (_sf_tracks_ and _sf_fingerprints_) in your `app.config` as outlined:
```xml
<configuration>
  <configSections>
    <section name="solr" type="Ninject.Integration.SolrNet.Config.SolrConfigurationSection, Ninject.Integration.SolrNet" />
  </configSections>
  <solr>
    <server id="tracks" url="http://localhost:8983/solr/sf_tracks" documentType="SoundFingerprinting.Solr.DAO.TrackDTO, SoundFingerprinting.Solr" />
    <server id="fingerprints" url="http://localhost:8983/solr/sf_fingerprints" documentType="SoundFingerprinting.Solr.DAO.SubFingerprintDTO, SoundFingerprinting.Solr" />
  </solr>
</configuration>

```

### Solr schema for fingerprints and tracks
_soundfingerprinting_ audio fingerprints are stored in _sf_fingerprints_ core. It's schema is outlined below (you can find it in `solr-config` folder:

```xml
<schema name="sf_fingerprints" version="1.5">
   
	<field name="subFingerprintId" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
	<field name="trackId" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
	<dynamicField name="hashTable_*" type="long" indexed="true" stored="true" omitTermFreqAndPositions="true" omitNorms="true" />
	<field name="sequenceAt" type="double" indexed="false" stored="true" required="false" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
	<field name="sequenceNumber" type="int" indexed="false" stored="true" required="false" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
	<field name="clusters" type="string" indexed="true" stored="true" required="false" multiValued="true" omitTermFreqAndPositions="true" omitNorms="true" />

</schema>
```

Track metadata is stored in _sf_tracks_ core.
```xml
<schema name="sf_fingerprints" version="1.5">
    <field name="trackId" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="artist" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="title" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="isrc" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="album" type="string" indexed="true" stored="true" required="true" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="trackLengthSec" type="double" indexed="false" stored="true" required="false" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
    <field name="releaseYear" type="int" indexed="false" stored="true" required="false" multiValued="false" omitTermFreqAndPositions="true" omitNorms="true" />
</schema>
```

### Disabling query result cache
It is important to disable query result cache for _sf_fingerprints_ core. You may end up with Solr memory issues otherwise.

### Third party dependencies
Links to the third party libraries used by _soundfingerprinting_ project.
* [SolrNet](https://github.com/mausch/SolrNet)
* [Ninject](http://www.ninject.org)

### Binaries
    git clone git@github.com:AddictedCS/soundfingerprinting.solr.git
    
In order to build latest version of the <code>SoundFingerprinting.Solr</code> assembly run the following command from repository root

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting.Solr -Pre
