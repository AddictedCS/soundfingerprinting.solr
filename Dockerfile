FROM    java:openjdk-8-jre
MAINTAINER  Sergiu Ciumac "ciumac.sergiu@gmail.com"

RUN apt-get update && \
  apt-get -y install lsof && \
  rm -rf /var/lib/apt/lists/*

ENV SOLR_USER solr
ENV SOLR_UID 8983

RUN groupadd -r -g $SOLR_UID $SOLR_USER && \
  useradd -r -u $SOLR_UID -g $SOLR_USER $SOLR_USER

ENV SOLR_VERSION 7.5.0
ENV SOLR_URL https://archive.apache.org/dist/lucene/solr/$SOLR_VERSION/solr-$SOLR_VERSION.tgz

RUN mkdir -p /opt/solr && \
  wget -nv $SOLR_URL -O /opt/solr.tgz && \
  wget -nv $SOLR_URL.asc -O /opt/solr.tgz.asc && \
  tar -C /opt/solr --extract --file /opt/solr.tgz --strip-components=1 && \
  rm /opt/solr.tgz* && \
  rm -Rf /opt/solr/docs/ && \
  mkdir -p /opt/solr/server/solr/lib && \
  chown -R $SOLR_USER:$SOLR_USER /opt/solr && \
  mkdir -p /opt/scripts && \
  chown -R $SOLR_USER:$SOLR_USER /opt/scripts

COPY solr-config/sf_fingerprints /opt/solr/server/solr/sf_fingerprints
COPY solr-config/sf_tracks /opt/solr/server/solr/sf_tracks
RUN chown -R $SOLR_USER:$SOLR_USER /opt/solr/server/solr

COPY docker/entrypoint.sh /opt/scripts
RUN chown -R $SOLR_USER:$SOLR_USER /opt/scripts

EXPOSE 8983
WORKDIR /opt/solr
USER $SOLR_USER

ENTRYPOINT ["/opt/scripts/entrypoint.sh"]
